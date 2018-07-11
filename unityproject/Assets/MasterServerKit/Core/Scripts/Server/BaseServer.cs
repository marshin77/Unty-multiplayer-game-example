// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Networking;

namespace MasterServerKit
{
    /// <summary>
    /// The base class used for all the servers in the kit. It abstracts away the code needed
    /// for handling player connections/disconnections.
    /// </summary>
    public class BaseServer : MonoBehaviour
    {
        /// <summary>
        /// The list of the players currently connected to this server.
        /// </summary>
        public List<Player> players = new List<Player>();

        /// <summary>
        /// Unity's Start method.
        /// </summary>
        protected virtual void Start()
        {
            NetworkServer.RegisterHandler(MsgType.Disconnect, OnDisconnected);
            NetworkServer.RegisterHandler(BaseServerNetworkProtocol.SendPlayerData, OnPlayerInfoReceived);
            SetupDefaultAddons();
        }

        /// <summary>
        /// Installs the default add-ons of this server. This method is intended to be overriden as needed
        /// by subclasses.
        /// </summary>
        protected virtual void SetupDefaultAddons()
        {
        }

        /// <summary>
        /// Handler for the Disconnect message.
        /// </summary>
        /// <param name="netMsg">The network message that was received.</param>
        protected virtual void OnDisconnected(NetworkMessage netMsg)
        {
            RemovePlayer(netMsg.conn);
            var serverAddons = GetComponents<ServerAddon>();
            if (serverAddons != null)
            {
                foreach (var addon in serverAddons)
                {
                    addon.OnDisconnectedFromBaseServer(netMsg.conn);
                }
            }
        }

        /// <summary>
        /// Handler for the SendPlayerData message.
        /// </summary>
        /// <param name="netMsg">The network message that was received.</param>
        public virtual void OnPlayerInfoReceived(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<SendPlayerDataMessage>();
            if (msg != null)
            {
                var player = players.Find(x => x.connection == netMsg.conn);
                if (player != null)
                {
                    player.name = msg.username;
                }
                else
                {
                    AddPlayer(netMsg.conn, msg.username);
                }
            }
        }

        /// <summary>
        /// Adds a new player with the specified network connection to this server.
        /// </summary>
        /// <param name="conn">The added player's network connection.</param>
        /// <param name="username">The added player's username.</param>
        public virtual void AddPlayer(NetworkConnection conn, string username)
        {
            var player = players.Find(x => x.connection == conn);
            if (player == null)
            {
                player = new Player(conn, username);
                players.Add(player);
            }
        }

        /// <summary>
        /// Removes an existing player with the specified network connection from this server.
        /// </summary>
        /// <param name="conn">The removed player's network connection.</param>
        public virtual void RemovePlayer(NetworkConnection conn)
        {
            var player = players.Find(x => x.connection == conn);
            if (player != null)
            {
                players.Remove(player);
            }
        }
    }
}
