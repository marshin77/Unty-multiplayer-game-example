// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using System.Diagnostics;

using UnityEngine;
using UnityEngine.Networking;

namespace MasterServerKit
{
    /// <summary>
    /// Stores the information of a spawned game room.
    /// </summary>
    public class SpawnedGame
    {
        public int id;
        public string ip;
        public int port;
        public string name;
        public int numPlayers;
        public int maxPlayers;
        public string password;
        public List<Property> properties = new List<Property>();
        public Process process;
        public NetworkClient zoneToGame;
    }

    /// <summary>
    /// Stores the information of a spawned game room that can be transmitted across
    /// the network.
    /// </summary>
    public class SpawnedGameNetwork
    {
        public int id;
        public string ip;
        public int port;
        public string name;
        public int numPlayers;
        public int maxPlayers;
        public bool isPrivate;
        public Property[] properties;
    }

    /// <summary>
    /// Server add-on that provides game rooms functionality for the zone server.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameRoomsZoneServerAddon : ServerAddon
    {
        /// <summary>
        /// The list of spawned game rooms.
        /// </summary>
        protected List<SpawnedGame> rooms = new List<SpawnedGame>();

        /// <summary>
        /// Registers the handlers for all the network messages this plugin is
        /// interested in.
        /// </summary>
        protected override void RegisterNetworkHandlers()
        {
            NetworkServer.RegisterHandler(GameRoomsNetworkProtocol.RequestCreateGameRoom, OnCreateGameRoomRequested);
            NetworkServer.RegisterHandler(GameRoomsNetworkProtocol.RequestDestroyGameRoom, OnDestroyGameRoomRequested);
        }

        /// <summary>
        /// Unregisters the handlers for all the network messages this plugin is
        /// interested in.
        /// </summary>
        protected override void UnregisterNetworkHandlers()
        {
            NetworkServer.UnregisterHandler(GameRoomsNetworkProtocol.RequestDestroyGameRoom);
            NetworkServer.UnregisterHandler(GameRoomsNetworkProtocol.RequestCreateGameRoom);
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
                var zoneServer = baseServer as ZoneServer;
                var properties = new List<Property>(zoneServer.properties);
                properties.AddRange(msg.properties);
                var spawnedGame = zoneServer.SpawnGameServer(msg.playerConnectionId, msg.name, msg.maxPlayers, msg.password, properties);
                var responseMsg = new ResponseSpawnGameServerMessage();
                if (spawnedGame != null)
                {
                    rooms.Add(spawnedGame);

                    responseMsg.success = true;
                    responseMsg.ip = spawnedGame.ip;
                    responseMsg.port = spawnedGame.port;
                }
                else
                {
                    responseMsg.success = false;
                }
                netMsg.conn.Send(ZoneServerNetworkProtocol.ResponseSpawnGameServer, responseMsg);
            }
        }

        /// <summary>
        /// Handler for the RequestDestroyGameRoom message.
        /// </summary>
        /// <param name="netMsg">Network message that was received.</param>
        protected virtual void OnDestroyGameRoomRequested(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<RequestDestroyGameRoomMessage>();
            if (msg != null)
            {
                var room = rooms.Find(x => x.port == msg.port);
                if (room != null)
                {
                    var zoneServer = baseServer as ZoneServer;
                    zoneServer.DestroyGameServer(room.port);
                    netMsg.conn.Send(GameRoomsNetworkProtocol.ResponseDestroyGameRoom, new ResponseDestroyGameRoomMessage());
                }
            }
        }
    }
}