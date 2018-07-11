// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Networking;

namespace MasterServerKit
{
    /// <summary>
    /// When using Master Server Kit together with UNET's Network Manager, we use a custom
    /// network manager in order to detect players connecting and disconnecting from the game
    /// server instead.
    /// </summary>
    public class MasterServerNetworkManager : NetworkManager
    {
        /// <summary>
        /// Cached reference to the game server.
        /// </summary>
        [HideInInspector]
        public GameServer gameServer;

        /// <summary>
        /// Called on the server when a client disconnects.
        /// </summary>
        /// <param name="conn">The disconnecting player's network connection.</param>
        public override void OnServerDisconnect(NetworkConnection conn)
        {
            base.OnServerDisconnect(conn);
            gameServer.RemovePlayer(conn);
        }
    }
}
