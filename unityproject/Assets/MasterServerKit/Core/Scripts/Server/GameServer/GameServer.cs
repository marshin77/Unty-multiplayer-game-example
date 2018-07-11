// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace MasterServerKit
{
    /// <summary>
    /// The game server is responsible for managing a single game session. Game servers can be:
    ///     - Independently launched, which is useful for world-based games.
    ///     - Dynamically spawned by the zone server, which is useful for room-based games.
    /// </summary>
    public class GameServer : BaseServer
    {
        /// <summary>
        /// The IP address of the master server.
        /// </summary>
        [Tooltip("The IP address of the master server.")]
        public string masterServerIp;

        /// <summary>
        /// The port of the master server.
        /// </summary>
        [Tooltip("The port of the master server.")]
        public int masterServerPort;

        /// <summary>
        /// The IP address of this game server.
        /// </summary>
        [Tooltip("The IP address of this game server.")]
        public string gameServerIp;

        /// <summary>
        /// The port of this game server.
        /// </summary>
        [Tooltip("The port of this game server.")]
        public int gameServerPort;

        /// <summary>
        /// The name of this game.
        /// </summary>
        [Tooltip("The name of this game.")]
        public string gameName;

        /// <summary>
        /// The maximum number of players allowed on this game.
        /// </summary>
        [Tooltip("The maximum number of players allowed on this game.")]
        public int maxPlayers;

        /// <summary>
        /// The password of this game (empty for public games).
        /// </summary>
        [Tooltip("The password of this game (empty for public games).")]
        public string password;

        /// <summary>
        /// True if this game server should be automatically registered in the master server upon launch;
        /// false otherwise.
        /// </summary>
        [Tooltip("True if this game server is standalone (meaning it is not spawned by a zone server); false otherwise.")]
        public bool isStandalone;

        /// <summary>
        /// True if this game server should be automatically closed when there are no connected players left; false otherwise.
        /// </summary>
        [Tooltip("True if this game server should be automatically closed when there are no connected players left; false otherwise.")]
        public bool closeWhenEmpty;

        /// <summary>
        /// True if this game server should be automatically removed from the matchmaking results when full; false otherwise.
        /// </summary>
        [Tooltip("True if this game server should be automatically removed from the matchmaking results when full; false otherwise.")]
        public bool hideWhenFull;

        /// <summary>
        /// True if this game server is using the Network Manager component available in UNET; false otherwise.
        /// </summary>
        [Tooltip("True if this game server is using the Network Manager component available in UNET; false otherwise.")]
        public bool useNetworkManager;

        /// <summary>
        /// The list of properties of this game server.
        /// </summary>
        public List<Property> properties;

        /// <summary>
        /// The network client used to communicate this game server with the master server.
        /// </summary>
        protected NetworkClient gameToMaster;

        /// <summary>
        /// The list of pending state updates to send to the master server.
        /// </summary>
        protected List<UpdateGameServerStateMessage> gameServerStateUpdates = new List<UpdateGameServerStateMessage>();

        /// <summary>
        /// The network client used to communicate this game server with the zone server that spawned it (if any).
        /// </summary>
        protected NetworkClient gameToZone;

        /// <summary>
        /// The IP address of the zone server that spawned this game server (if any).
        /// </summary>
        protected string zoneServerIp;

        /// <summary>
        /// The port of the zone server that spawned this game server (if any).
        /// </summary>
        protected int zoneServerPort;

        /// <summary>
        /// The host id of the WebGL server.
        /// </summary>
        public static int webGLHostId { get; protected set; }

        /// <summary>
        /// The server used to handle WebGL connections.
        /// </summary>
        protected NetworkServerSimple webGLServer = new NetworkServerSimple();

        /// <summary>
        /// The connection id of the player requesting the creation of this game server.
        /// </summary>
        protected int playerConnectionId;

        /// <summary>
        /// Unity's Start method.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            if (isStandalone)
            {
                StartListening();
                RegisterInMasterServer();
            }
            else
            {
                var args = Environment.GetCommandLineArgs();
                var ip = args[1];
                var port = int.Parse(args[2]);
                gameServerIp = ip;
                gameServerPort = port;
                StartListening();
            }
        }

        /// <summary>
        /// Unity's Update method.
        /// </summary>
        protected virtual void Update()
        {
            if (gameToMaster != null && gameToMaster.isConnected)
            {
                var updatesToRemove = new List<UpdateGameServerStateMessage>();

                foreach (var update in gameServerStateUpdates)
                {
                    gameToMaster.Send(GameRoomsNetworkProtocol.UpdateGameServerState, update);
                    updatesToRemove.Add(update);
                }

                foreach (var update in updatesToRemove)
                {
                    gameServerStateUpdates.Remove(update);
                }
            }

            if (!useNetworkManager)
            {
                webGLServer.Update();
            }
        }

        /// <summary>
        /// Installs the default add-ons of this server. You can override this method in
        /// a subclass if you need a custom configuration.
        /// </summary>
        protected override void SetupDefaultAddons()
        {
            gameObject.AddComponent<ChatServerAddon>();
        }

        /// <summary>
        /// Starts the game server.
        /// </summary>
        protected virtual void StartListening()
        {
            if (useNetworkManager)
            {
                DontDestroyOnLoad(gameObject);
                var networkManager = NetworkManager.singleton as MasterServerNetworkManager;
                Assert.IsTrue(networkManager != null, "A Master Server Network Manager was not found.");
                networkManager.gameServer = this;
                networkManager.networkAddress = gameServerIp;
                networkManager.networkPort = gameServerPort;
                networkManager.StartServer();
                NetworkServer.RegisterHandler(GameServerNetworkProtocol.RequestGameServerRegistration, OnGameServerRegistrationRequested);
                NetworkServer.RegisterHandler(BaseServerNetworkProtocol.SendPlayerData, OnPlayerInfoReceived);
            }
            else
            {
                webGLServer.SetNetworkConnectionClass<GameServerWebGLCustomNetworkConnection>();
                webGLServer.useWebSockets = true;
                webGLServer.RegisterHandler(MsgType.Connect, OnWebGLConnected);
                webGLServer.RegisterHandler(MsgType.Disconnect, OnWebGLDisconnected);
                webGLServer.Listen(gameServerIp, gameServerPort);
                webGLHostId = webGLServer.serverHostId;

                NetworkServer.Listen(gameServerIp, gameServerPort);
                NetworkServer.RegisterHandler(GameServerNetworkProtocol.RequestGameServerRegistration, OnGameServerRegistrationRequested);
            }

            if (closeWhenEmpty)
            {
                StartCoroutine(CloseWhenEmptyService());
            }
        }

        /// <summary>
        /// Notifies the master server that this game server is ready and listening.
        /// </summary>
        protected virtual void NotifyGameServerIsReady()
        {
            var msg = new NotifyGameServerReadyMessage();
            msg.playerConnectionId = playerConnectionId;
            msg.ip = gameServerIp;
            msg.port = gameServerPort;
            gameToMaster.Send(GameRoomsNetworkProtocol.NotifyGameServerReady, msg);
        }

        /// <summary>
        /// Handler for the Connect message of WebGL clients.
        /// </summary>
        /// <param name="netMsg">The network message that was received.</param>
        protected virtual void OnWebGLConnected(NetworkMessage netMsg)
        {
            NetworkServer.AddExternalConnection(netMsg.conn);
        }

        /// <summary>
        /// Handler for the Disconnect message of WebGL clients.
        /// </summary>
        /// <param name="netMsg">The network message that was received.</param>
        protected virtual void OnWebGLDisconnected(NetworkMessage netMsg)
        {
            NetworkServer.RemoveExternalConnection(netMsg.conn.connectionId);
        }

        /// <summary>
        /// Registers this game server in the master server.
        /// </summary>
        protected virtual void RegisterInMasterServer()
        {
            gameToMaster = new NetworkClient();
            gameToMaster.RegisterHandler(MsgType.Connect, netMsg =>
            {
                var msg = new RegisterGameServerMessage();
                msg.ip = gameServerIp;
                msg.port = gameServerPort;
                msg.name = gameName;
                msg.maxPlayers = maxPlayers;
                msg.password = password;
                msg.hideWhenFull = hideWhenFull;
                msg.properties = properties.ToArray();
                gameToMaster.Send(GameRoomsNetworkProtocol.RegisterGameServer, msg);

                NotifyGameServerIsReady();
            });
            gameToMaster.Connect(masterServerIp, masterServerPort);
        }

        /// <summary>
        /// Handler for the RequestGameServerRegistrationMessage message.
        /// </summary>
        /// <param name="netMsg">The network message that was received.</param>
        protected virtual void OnGameServerRegistrationRequested(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<RequestGameServerRegistrationMessage>();
            if (msg != null)
            {
                playerConnectionId = msg.playerConnectionId;
                zoneServerIp = msg.zoneServerIp;
                zoneServerPort = msg.zoneServerPort;
                gameName = msg.name;
                maxPlayers = msg.maxPlayers;
                password = msg.password;
                foreach (var property in msg.properties)
                {
                    properties.Add(property);
                }

                RegisterInMasterServer();
                netMsg.conn.Send(GameServerNetworkProtocol.ResponseGameServerRegistration, new ResponseGameServerRegistrationMessage());
            }
        }

        /// <summary>
        /// This service ensures the game server is shut down when there are
        /// no connected players left.
        /// </summary>
        /// <returns>Coroutine for the service.</returns>
        protected virtual IEnumerator CloseWhenEmptyService()
        {
            while (true)
            {
                yield return new WaitForSeconds(30.0f);
                CloseWhenEmpty();
            }
        }

        /// <summary>
        /// Closes this game server.
        /// </summary>
        public void Shutdown()
        {
            Application.Quit();
        }

        /// <summary>
        /// Shuts this server down if there are no connected players left.
        /// </summary>
        public void CloseWhenEmpty()
        {
            if (players.Count == 0)
            {
                if (!isStandalone)
                {
                    gameToZone = new NetworkClient();
                    var destroyMsg = new RequestDestroyGameRoomMessage();
                    destroyMsg.port = gameServerPort;
                    gameToZone.RegisterHandler(MsgType.Connect, x => gameToZone.Send(GameRoomsNetworkProtocol.RequestDestroyGameRoom, destroyMsg));
                    gameToZone.RegisterHandler(GameRoomsNetworkProtocol.ResponseDestroyGameRoom, x => gameToZone.Disconnect());
                    gameToZone.Connect(zoneServerIp, zoneServerPort);
                }

                var msg = new UnregisterGameServerMessage();
                gameToMaster.RegisterHandler(GameRoomsNetworkProtocol.UnregisterGameServerResponse, x => gameToMaster.Disconnect());
                gameToMaster.Send(GameRoomsNetworkProtocol.UnregisterGameServer, msg);

                Invoke("Shutdown", 5.0f);
            }
        }

        /// <summary>
        /// Adds a new player with the specified network connection to this server.
        /// </summary>
        /// <param name="conn">The added player's network connection.</param>
        /// <param name="username">The added player's username.</param>
        public override void AddPlayer(NetworkConnection conn, string username)
        {
            base.AddPlayer(conn, username);
            var msg = new UpdateGameServerStateMessage();
            msg.numPlayers = players.Count;
            gameServerStateUpdates.Add(msg);
        }

        /// <summary>
        /// Removes an existing player with the specified network connection from this server.
        /// </summary>
        /// <param name="conn">The removed player's network connection.</param>
        public override void RemovePlayer(NetworkConnection conn)
        {
            var oldNumPlayers = players.Count;

            base.RemovePlayer(conn);
            var msg = new UpdateGameServerStateMessage();
            msg.numPlayers = players.Count;
            gameServerStateUpdates.Add(msg);

            if (oldNumPlayers != players.Count)
            {
                if (closeWhenEmpty)
                {
                    CloseWhenEmpty();
                }
            }
        }
    }
}