// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Networking;

using RSG;

namespace MasterServerKit
{
    /// <summary>
    /// This class stores the information of a zone server that is registered in the master server.
    /// </summary>
    public class RegisteredZoneServer
    {
        public NetworkConnection connection;
        public string ip;
        public int port;

        public RegisteredZoneServer(NetworkConnection connection, string ip, int port)
        {
            this.connection = connection;
            this.ip = ip;
            this.port = port;
        }
    }

    /// <summary>
    /// This class stores the information of a game server that is registered in the master server.
    /// </summary>
    public class RegisteredGameServer
    {
        public NetworkConnection connection;
        public int id;
        public string ip;
        public int port;
        public string name;
        public int numPlayers;
        public int maxPlayers;
        public string password;
        public bool hideWhenFull;
        public List<Property> properties = new List<Property>();
    }

    /// <summary>
    /// Utility class for representing a pair of (IP address, port).
    /// </summary>
    public class ServerConnectionInfo
    {
        public string ip;
        public int port;
    }

    /// <summary>
    /// Server add-on that provides game rooms functionality for the master server.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameRoomsMasterServerAddon : ServerAddon
    {
        /// <summary>
        /// The list of registered zone servers.
        /// </summary>
        protected List<RegisteredZoneServer> registeredZoneServers = new List<RegisteredZoneServer>();

        /// <summary>
        /// The list of registered game servers.
        /// </summary>
        protected List<RegisteredGameServer> registeredGameServers = new List<RegisteredGameServer>();

        /// <summary>
        /// The list of player connections that have requested the creation of a new game room and are awaiting
        /// for a response.
        /// </summary>
        protected List<NetworkConnection> awaitingPlayerConnections = new List<NetworkConnection>();

        /// <summary>
        /// The current unique identifier to use for the next game room that is spawned.
        /// </summary>
        protected int gameServerId;

        /// <summary>
        /// Registers the handlers for all the network messages this add-on is interested in.
        /// </summary>
        protected override void RegisterNetworkHandlers()
        {
            // Player-related network handlers.
            NetworkServer.RegisterHandler(GameRoomsNetworkProtocol.RequestPing, OnPingRequested);
            NetworkServer.RegisterHandler(GameRoomsNetworkProtocol.RequestFindGameRooms, OnFindGameRoomsRequested);
            NetworkServer.RegisterHandler(GameRoomsNetworkProtocol.RequestCreateGameRoom, OnCreateGameRoomRequested);
            NetworkServer.RegisterHandler(GameRoomsNetworkProtocol.RequestJoinGameRoom, OnJoinGameRoomRequested);
            NetworkServer.RegisterHandler(GameRoomsNetworkProtocol.RequestPlayNow, OnPlayNowRequested);

            // Zone registration network handlers.
            NetworkServer.RegisterHandler(GameRoomsNetworkProtocol.RegisterZoneServer, OnZoneServerRegistered);

            // Game registration network handlers.
            NetworkServer.RegisterHandler(GameRoomsNetworkProtocol.RegisterGameServer, OnGameServerRegistered);
            NetworkServer.RegisterHandler(GameRoomsNetworkProtocol.UnregisterGameServer, OnGameServerUnregistered);
            NetworkServer.RegisterHandler(GameRoomsNetworkProtocol.UpdateGameServerState, OnGameServerStateUpdated);
            NetworkServer.RegisterHandler(GameRoomsNetworkProtocol.NotifyGameServerReady, OnNotifiedGameServerReady);
        }

        /// <summary>
        /// Unregisters the handlers for all the network messages this add-on is interested in.
        /// </summary>
        protected override void UnregisterNetworkHandlers()
        {
            NetworkServer.UnregisterHandler(GameRoomsNetworkProtocol.NotifyGameServerReady);
            NetworkServer.UnregisterHandler(GameRoomsNetworkProtocol.UpdateGameServerState);
            NetworkServer.UnregisterHandler(GameRoomsNetworkProtocol.UnregisterGameServer);
            NetworkServer.UnregisterHandler(GameRoomsNetworkProtocol.RegisterGameServer);

            NetworkServer.UnregisterHandler(GameRoomsNetworkProtocol.RegisterZoneServer);

            NetworkServer.UnregisterHandler(GameRoomsNetworkProtocol.RequestPlayNow);
            NetworkServer.UnregisterHandler(GameRoomsNetworkProtocol.RequestJoinGameRoom);
            NetworkServer.UnregisterHandler(GameRoomsNetworkProtocol.RequestCreateGameRoom);
            NetworkServer.UnregisterHandler(GameRoomsNetworkProtocol.RequestFindGameRooms);
            NetworkServer.UnregisterHandler(GameRoomsNetworkProtocol.RequestPing);
        }

        /// <summary>
        /// Called when a player is disconnected from the base server.
        /// </summary>
        /// <param name="connection">The network connection of the disconnected player.</param>
        public override void OnDisconnectedFromBaseServer(NetworkConnection connection)
        {
            UnregisterZoneServer(connection);
            UnregisterGameServer(connection);
        }

        /// <summary>
        /// Handler for the RequestPing message.
        /// </summary>
        /// <param name="netMsg">Network message that was received.</param>
        protected virtual void OnPingRequested(NetworkMessage netMsg)
        {
            var msg = new ResponsePingMessage();
            msg.numConnectedPlayers = baseServer.players.Count;
            netMsg.conn.Send(GameRoomsNetworkProtocol.ResponsePing, msg);
        }

        /// <summary>
        /// Handler for the RequestFindGameRooms message.
        /// </summary>
        /// <param name="netMsg">Network message that was received.</param>
        protected virtual void OnFindGameRoomsRequested(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<RequestFindGameRoomsMessage>();
            if (msg != null)
            {
                var responseMsg = new ResponseFindGameRoomsMessage();
                var responseGames = new List<SpawnedGameNetwork>();
                foreach (var server in registeredGameServers)
                {
                    if (!ServerIsHiddenFromMatchmaking(server) &&
                        ServerIncludesProperties(server, msg.includeProperties) &&
                        ServerExcludesProperties(server, msg.excludeProperties))
                    {
                        var gameInfo = new SpawnedGameNetwork();
                        gameInfo.id = server.id;
                        gameInfo.ip = server.ip;
                        gameInfo.port = server.port;
                        gameInfo.name = server.name;
                        gameInfo.numPlayers = server.numPlayers;
                        gameInfo.maxPlayers = server.maxPlayers;
                        gameInfo.isPrivate = !string.IsNullOrEmpty(server.password);
                        gameInfo.properties = server.properties.ToArray();
                        responseGames.Add(gameInfo);
                    }
                }
                responseMsg.games = responseGames.ToArray();

                netMsg.conn.Send(GameRoomsNetworkProtocol.ResponseFindGameRooms, responseMsg);
            }
        }

        /// <summary>
        /// Checks if the specified server is hidden from the matchmaking results.
        /// </summary>
        /// <param name="server">Server to check.</param>
        /// <returns>True if the specified server is hidden from the matchmaking results; false otherwise.</returns>
        protected virtual bool ServerIsHiddenFromMatchmaking(RegisteredGameServer server)
        {
            return (server.hideWhenFull && server.numPlayers >= server.maxPlayers);
        }

        /// <summary>
        /// Checks if the specified server includes all the specified properties.
        /// </summary>
        /// <param name="server">Server to check.</param>
        /// <param name="properties">Properties to check.</param>
        /// <returns>True if the specified server includes all the specified properties; false otherwise.</returns>
        protected virtual bool ServerIncludesProperties(RegisteredGameServer server, Property[] properties)
        {
            foreach (var property in properties)
            {
                if (server.properties.Find(x => x.name == property.name && x.value == property.value) == null)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Checks if the specified server excludes all the specified properties.
        /// </summary>
        /// <param name="server">Server to check.</param>
        /// <param name="properties">Properties to check.</param>
        /// <returns>True if the specified server excludes all the specified properties; false otherwise.</returns>
        protected virtual bool ServerExcludesProperties(RegisteredGameServer server, Property[] properties)
        {
            foreach (var property in properties)
            {
                if (server.properties.Find(x => x.name == property.name && x.value == property.value) != null)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Handler for the RequestCreateGameRoom message.
        /// </summary>
        /// <param name="netMsg">Network message that was received.</param>
        protected virtual void OnCreateGameRoomRequested(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<RequestCreateGameRoomMessage>();
            if (msg != null)
            {
                CreateGameRoom(netMsg.conn, msg);
            }
        }

        /// <summary>
        /// Handler for the RequestJoinGameRoom message.
        /// </summary>
        /// <param name="netMsg">Network message that was received.</param>
        protected virtual void OnJoinGameRoomRequested(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<RequestJoinGameRoomMessage>();
            if (msg != null)
            {
                var gameServer = registeredGameServers.Find(x => x.id == msg.id);
                var responseMsg = new ResponseJoinGameRoomMessage();
                var success = true;
                if (gameServer != null)
                {
                    if (!string.IsNullOrEmpty(gameServer.password))
                    {
                        if (msg.password != gameServer.password)
                        {
                            success = false;
                            responseMsg.error = JoinGameRoomError.InvalidPassword;
                        }
                    }

                    if (gameServer.numPlayers == gameServer.maxPlayers)
                    {
                        success = false;
                        responseMsg.error = JoinGameRoomError.GameFull;
                    }
                }
                else
                {
                    success = false;
                    responseMsg.error = JoinGameRoomError.GameExpired;
                }

                if (success)
                {
                    responseMsg.ip = gameServer.ip;
                    responseMsg.port = gameServer.port;
                }

                responseMsg.success = success;
                netMsg.conn.Send(GameRoomsNetworkProtocol.ResponseJoinGameRoom, responseMsg);
            }
        }

        /// <summary>
        /// Handler for the RequestPlayNow message.
        /// </summary>
        /// <param name="netMsg">Network message that was received.</param>
        protected virtual void OnPlayNowRequested(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<RequestPlayNowMessage>();
            if (msg != null)
            {
                var game = registeredGameServers.Find(x => string.IsNullOrEmpty(x.password) && x.numPlayers < x.maxPlayers);
                if (game != null)
                {
                    var responseMsg = new ResponseJoinGameRoomMessage();
                    responseMsg.success = true;
                    responseMsg.ip = game.ip;
                    responseMsg.port = game.port;
                    netMsg.conn.Send(GameRoomsNetworkProtocol.ResponseJoinGameRoom, responseMsg);
                }
                else
                {
                    var requestMsg = new RequestCreateGameRoomMessage();
                    requestMsg.playerConnectionId = netMsg.conn.connectionId;
                    requestMsg.name = "Default game";
                    requestMsg.maxPlayers = 4;
                    requestMsg.password = null;
                    CreateGameRoom(netMsg.conn, requestMsg);
                }
            }
        }

        /// <summary>
        /// Creates a new game room.
        /// </summary>
        /// <param name="playerConnection">The network connection of the player requesting the creation of a game room.</param>
        /// <param name="msg">The network message containing the information of the game room to create.</param>
        protected virtual void CreateGameRoom(NetworkConnection playerConnection, RequestCreateGameRoomMessage msg)
        {
            var zoneServer = SelectZoneServer();
            if (zoneServer != null)
            {
                ConnectToZoneServer(zoneServer.ip, zoneServer.port)
                    .Then(client =>
                    {
                        awaitingPlayerConnections.Add(playerConnection);
                        msg.playerConnectionId = playerConnection.connectionId;
                        return RequestCreateGameRoom(client, msg);
                    })
                    .Then(responseSpawnGameServerMessage =>
                    {
                        if (!responseSpawnGameServerMessage.success)
                        {
                            var connection = awaitingPlayerConnections.Find(x => x.connectionId == responseSpawnGameServerMessage.playerConnectionId);
                            if (connection != null)
                            {
                                var responseMsg = new ResponseCreateGameRoomMessage();
                                responseMsg.success = false;
                                connection.Send(GameRoomsNetworkProtocol.ResponseCreateGameRoom, responseMsg);
                                awaitingPlayerConnections.Remove(connection);
                            }
                        }
                    })
                    .Catch(e =>
                    {
                        Debug.Log(e.Message);
                    });
            }
            else
            {
                var responseMsg = new ResponseCreateGameRoomMessage();
                responseMsg.success = false;
                responseMsg.error = CreateGameRoomError.ZoneServerUnavailable;
                playerConnection.Send(GameRoomsNetworkProtocol.ResponseCreateGameRoom, responseMsg);
            }
        }

        /// <summary>
        /// Randomly selects a zone server to spawn a new game room.
        /// </summary>
        /// <returns>A random zone server to spawn a new game room.</returns>
        protected virtual RegisteredZoneServer SelectZoneServer()
        {
            if (registeredZoneServers.Count > 0)
            {
                var idx = Random.Range(0, registeredZoneServers.Count);
                return registeredZoneServers[idx];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a connection to the zone server at the specified IP address and port.
        /// </summary>
        /// <param name="ip">IP address of the zone server to connect to.</param>
        /// <param name="port">Port of the zone server to connect to.</param>
        /// <returns>A promise with the connected network client.</returns>
        protected IPromise<NetworkClient> ConnectToZoneServer(string ip, int port)
        {
            var promise = new Promise<NetworkClient>();
            var client = new NetworkClient();
            client.RegisterHandler(MsgType.Connect, x => { promise.Resolve(client); });
            client.RegisterHandler(MsgType.Disconnect, x => { if (promise.CurState == PromiseState.Pending) promise.Reject(new System.Exception()); });
            client.Connect(ip, port);
            return promise;
        }

        /// <summary>
        /// Requests the creation of a game room to the zone server.
        /// </summary>
        /// <param name="client">The network client connected to the zone server.</param>
        /// <param name="msg">The network message containing the information of the game room to create.</param>
        /// <returns>A promise with the network response to the game server spawn.</returns>
        protected IPromise<ResponseSpawnGameServerMessage> RequestCreateGameRoom(NetworkClient client, RequestCreateGameRoomMessage msg)
        {
            var promise = new Promise<ResponseSpawnGameServerMessage>();
            client.RegisterHandler(ZoneServerNetworkProtocol.ResponseSpawnGameServer, netMsg =>
            {
                var responseMsg = netMsg.ReadMessage<ResponseSpawnGameServerMessage>();
                client.Disconnect();
                promise.Resolve(responseMsg);
            });
            client.Send(GameRoomsNetworkProtocol.RequestCreateGameRoom, msg);
            return promise;
        }

        /// <summary>
        /// Handler for the Disconnect message.
        /// </summary>
        /// <param name="netMsg">Network message that was received.</param>
        protected virtual void OnZoneServerDisconnected(NetworkMessage netMsg)
        {
            UnregisterZoneServer(netMsg.conn);
        }

        /// <summary>
        /// Handler for the RegisterZoneServer message.
        /// </summary>
        /// <param name="netMsg">Network message that was received.</param>
        protected virtual void OnZoneServerRegistered(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<RegisterZoneServerMessage>();
            if (msg != null)
            {
                RegisterZoneServer(netMsg.conn, msg.ip, msg.port);
            }
        }

        /// <summary>
        /// Registers the zone server with the specified information in the master server.
        /// </summary>
        /// <param name="connection">Network connection of the zone server to register.</param>
        /// <param name="ip">IP address of the zone server to register.</param>
        /// <param name="port">Port of the zone server to register.</param>
        protected virtual void RegisterZoneServer(NetworkConnection connection, string ip, int port)
        {
            var zoneServer = new RegisteredZoneServer(connection, ip, port);
            registeredZoneServers.Add(zoneServer);
            Debug.Log("Registered zone server with IP address = " + zoneServer.ip + " and port = " + zoneServer.port + ".");
        }

        /// <summary>
        /// Unregisters the zone server with the specified network connection from the master server.
        /// </summary>
        /// <param name="connection">Network connection of the zone server to unregister.</param>
        protected virtual void UnregisterZoneServer(NetworkConnection connection)
        {
            var zoneServer = registeredZoneServers.Find(x => x.connection == connection);
            if (zoneServer != null)
            {
                registeredZoneServers.Remove(zoneServer);
                Debug.Log("Unregistered zone server with IP address = " + zoneServer.ip + " and port = " + zoneServer.port + ".");
            }
        }

        /// <summary>
        /// Handler for the RegisterGameServer message.
        /// </summary>
        /// <param name="netMsg">Network message that was received.</param>
        protected virtual void OnGameServerRegistered(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<RegisterGameServerMessage>();
            if (msg != null)
            {
                RegisterGameServer(netMsg.conn, msg);
                foreach (var property in msg.properties)
                {
                    Debug.Log(property.name + " = " + property.value);
                }
            }
        }

        /// <summary>
        /// Handler for the UnregisterGameServer message.
        /// </summary>
        /// <param name="netMsg">Network message that was received.</param>
        protected virtual void OnGameServerUnregistered(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<UnregisterGameServerMessage>();
            if (msg != null)
            {
                UnregisterGameServer(netMsg.conn);
                netMsg.conn.Send(GameRoomsNetworkProtocol.UnregisterGameServerResponse, new UnregisterGameServerResponseMessage());
            }
        }

        /// <summary>
        /// Registers the game server with the specified information in the master server.
        /// </summary>
        /// <param name="connection">Network connection of the game server to register.</param>
        /// <param name="msg">The network message containing the information of the game server to register.</param>
        protected virtual void RegisterGameServer(NetworkConnection connection, RegisterGameServerMessage msg)
        {
            var gameServer = new RegisteredGameServer();
            gameServer.connection = connection;
            gameServer.id = gameServerId++;
            gameServer.ip = msg.ip;
            gameServer.port = msg.port;
            gameServer.name = msg.name;
            gameServer.maxPlayers = msg.maxPlayers;
            gameServer.password = msg.password;
            gameServer.hideWhenFull = msg.hideWhenFull;
            gameServer.properties = new List<Property>(msg.properties);
            registeredGameServers.Add(gameServer);
            Debug.Log("Registered game server (id = " + gameServer.id + ") with IP address = " + gameServer.ip + " and port = " + gameServer.port + ".");
        }

        /// <summary>
        /// Unregisters the game server with the specified network connection from the master server.
        /// </summary>
        /// <param name="connection">Network connection of the game server to unregister.</param>
        protected virtual void UnregisterGameServer(NetworkConnection connection)
        {
            var gameServer = registeredGameServers.Find(x => x.connection == connection);
            if (gameServer != null)
            {
                registeredGameServers.Remove(gameServer);
                Debug.Log("Unregistered game server (id = " + gameServer.id + ") with IP address = " + gameServer.ip + " and port = " + gameServer.port + ".");
            }
        }

        /// <summary>
        /// Handler for the UpdateGameServerState message.
        /// </summary>
        /// <param name="netMsg">Network message that was received.</param>
        protected virtual void OnGameServerStateUpdated(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<UpdateGameServerStateMessage>();
            if (msg != null)
            {
                var gameServer = registeredGameServers.Find(x => x.connection == netMsg.conn);
                if (gameServer != null)
                {
                    gameServer.numPlayers = msg.numPlayers;
                }
            }
        }

        /// <summary>
        /// Handler for the NotifyGameServerReady message.
        /// </summary>
        /// <param name="netMsg">Network message that was received.</param>
        protected virtual void OnNotifiedGameServerReady(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<NotifyGameServerReadyMessage>();
            if (msg != null)
            {
                var connection = awaitingPlayerConnections.Find(x => x.connectionId == msg.playerConnectionId);
                if (connection != null)
                {
                    var responseMsg = new ResponseCreateGameRoomMessage();
                    responseMsg.success = true;
                    responseMsg.ip = msg.ip;
                    responseMsg.port = msg.port;
                    connection.Send(GameRoomsNetworkProtocol.ResponseCreateGameRoom, responseMsg);
                    awaitingPlayerConnections.Remove(connection);
                }
            }
        }
    }
}