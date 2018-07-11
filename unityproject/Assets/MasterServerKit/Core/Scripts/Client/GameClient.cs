// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine.Networking;

namespace MasterServerKit
{
    /// <summary>
    /// This network client is used to connect the player to a spawned game server.
    /// </summary>
    public class GameClient : BaseNetworkClient
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="client">Network client.</param>
        /// <param name="username">Username.</param>
        public GameClient(NetworkClient client, string username) : base(client, username)
        {
        }

        /// <summary>
        /// This method is called when this client is connected to a server.
        /// </summary>
        protected override void OnConnected()
        {
            var msg = new SendPlayerDataMessage();
            msg.username = username;
            client.Send(BaseServerNetworkProtocol.SendPlayerData, msg);
        }

        /// <summary>
        /// This method is called when this client is disconnected from a server.
        /// </summary>
        protected override void OnDisconnected()
        {
            ClientAPI.gameClient = null;
        }
    }
}
