// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections.Generic;
using System.Diagnostics;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace MasterServerKit
{
    /// <summary>
    /// The zone server is responsible for spawning new game server instances in response to
    /// requests made by the master server. Zone servers are automatically registered in the
    /// master server upon launch and they are useful for implementing functionality like
    /// load balancing and region-based matchmaking in your networking architecture.
    /// </summary>
    public class ZoneServer : BaseServer
    {
        /// <summary>
        /// The IP address of the master server.
        /// </summary>
        [Tooltip("The IP address of the master server.")]
        public string masterServerIp = "127.0.0.1";

        /// <summary>
        /// The port of the master server.
        /// </summary>
        [Tooltip("The port of the master server.")]
        public int masterServerPort = 8000;

        /// <summary>
        /// The IP address of the zone server.
        /// </summary>
        [Tooltip("The IP address of the zone server.")]
        public string zoneServerIp = "127.0.0.1";

        /// <summary>
        /// The port of the zone server.
        /// </summary>
        [Tooltip("The port of the zone server.")]
        public int zoneServerPort = 9000;

        /// <summary>
        /// The path where the game server binary is located.
        /// </summary>
        [Tooltip("The path where the game server binary is located.")]
        public string gameServerBinaryPath;

        /// <summary>
        /// The maximum number of games servers that can be spawned on this zone server.
        /// </summary>
        [Tooltip("The maximum number of game servers that can be spawned on this zone server.")]
        public int maxGameServers = 100;

        /// <summary>
        /// The number of game servers that are automatically spawned when this zone server is launched.
        /// </summary>
        [Tooltip("The number of game servers that are automatically spawned when this zone server is launched.")]
        public int numPreSpawnedGameServers;

        /// <summary>
        /// True if the game servers should be spawned with the -batchmode -nographics command line arguments;
        /// false otherwise.
        /// </summary>
        [Tooltip("True if the game servers should be spawned with the -batchmode -nographics command line arguments; false otherwise.")]
        public bool spawnGameServersInBatchMode;

        /// <summary>
        /// The default name of the pre-spawned game servers.
        /// </summary>
        [Tooltip("The default name of the pre-spawned game servers.")]
        public string defaultGameName;

        /// <summary>
        /// The default maximum number of players of the pre-spawned game servers.
        /// </summary>
        [Tooltip("The default maximum number of players of the pre-spawned game servers.")]
        public int defaultGameMaxPlayers;

        /// <summary>
        /// The list of properties of this zone server.
        /// </summary>
        public List<Property> properties;

        /// <summary>
        /// The network client used to communicate this zone server with the master server.
        /// </summary>
        protected NetworkClient zoneToMaster = new NetworkClient();

        /// <summary>
        /// The queue of ports available to be used by newly-spawned game server instances.
        /// </summary>
        protected Queue<int> availablePorts = new Queue<int>();

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        public virtual void Awake()
        {
            Assert.IsTrue(!string.IsNullOrEmpty(masterServerIp));
            Assert.IsTrue(!string.IsNullOrEmpty(zoneServerIp));

            for (var i = 0; i < maxGameServers; i++)
            {
                availablePorts.Enqueue(zoneServerPort + i + 1);
            }
        }

        /// <summary>
        /// Unity's Start method.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            NetworkServer.Listen(zoneServerIp, zoneServerPort);

            // Register this zone server in the master server.
            zoneToMaster.RegisterHandler(MsgType.Connect, x =>
            {
                var msg = new RegisterZoneServerMessage();
                msg.ip = zoneServerIp;
                msg.port = zoneServerPort;
                zoneToMaster.Send(GameRoomsNetworkProtocol.RegisterZoneServer, msg);
            });
            zoneToMaster.Connect(masterServerIp, masterServerPort);

            // Spawn pre-spawned game servers.
            for (var i = 0; i < numPreSpawnedGameServers; i++)
            {
                SpawnGameServer(-1, defaultGameName, defaultGameMaxPlayers, null, properties);
            }
        }

        /// <summary>
        /// Installs the default add-ons of this server. You can override this method in
        /// a subclass if you need a custom configuration.
        /// </summary>
        protected override void SetupDefaultAddons()
        {
            gameObject.AddComponent<GameRoomsZoneServerAddon>();
        }

        /// <summary>
        /// Spawns a new game server instance with the specified information on this zone server.
        /// </summary>
        /// <param name="playerConnectionId">The connection id of the player requesting the creation of this game.</param>
        /// <param name="name">The name of the game server.</param>
        /// <param name="maxPlayers">The maximum number of players allowed on the game server.</param>
        /// <param name="password">The password of the game server.</param>
        /// <param name="properties">The properties of the game server.</param>
        /// <returns></returns>
        public virtual SpawnedGame SpawnGameServer(int playerConnectionId, string name, int maxPlayers, string password, List<Property> properties)
        {
            var ip = zoneServerIp;
            var port = availablePorts.Dequeue();
            try
            {
                var arguments = ip + " " + port;
                if (spawnGameServersInBatchMode)
                {
                    arguments += " -batchmode -nographics";
                }
                var process = Process.Start(gameServerBinaryPath, arguments);
                var gameServer = new SpawnedGame();
                gameServer.ip = ip;
                gameServer.port = port;
                gameServer.process = process;
                gameServer.name = name;
                gameServer.maxPlayers = maxPlayers;
                gameServer.password = password;
                gameServer.properties = properties;

                var zoneToGame = new NetworkClient();
                zoneToGame.RegisterHandler(MsgType.Scene, x => { });
                zoneToGame.RegisterHandler(MsgType.Connect, x =>
                {
                    var requestMsg = new RequestGameServerRegistrationMessage();
                    requestMsg.playerConnectionId = playerConnectionId;
                    requestMsg.zoneServerIp = zoneServerIp;
                    requestMsg.zoneServerPort = zoneServerPort;
                    requestMsg.name = name;
                    requestMsg.maxPlayers = maxPlayers;
                    requestMsg.password = password;
                    requestMsg.properties = properties.ToArray();
                    zoneToGame.Send(GameServerNetworkProtocol.RequestGameServerRegistration, requestMsg);
                });
                zoneToGame.RegisterHandler(GameServerNetworkProtocol.ResponseGameServerRegistration, x => zoneToGame.Disconnect());
                zoneToGame.Connect(gameServer.ip, gameServer.port);

                return gameServer;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Destroys the game server instance with the specified port.
        /// </summary>
        /// <param name="port">The port of the game server instance to destroy.</param>
        public virtual void DestroyGameServer(int port)
        {
            availablePorts.Enqueue(port);
        }
    }
}