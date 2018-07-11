// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine.Networking;

namespace MasterServerKit
{
    /// <summary>
    /// Stores the data for a player connected to the master server.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// The network connection associated to this player.
        /// </summary>
        public NetworkConnection connection { get; private set; }

        /// <summary>
        /// The name of this player.
        /// </summary>
        public string name;

        /// <summary>
        /// The attributes of this player.
        /// </summary>
        public Dictionary<string, string> attributes = new Dictionary<string, string>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="connection">The network connection associated to this player.</param>
        /// <param name="name">The name of this player.</param>
        public Player(NetworkConnection connection, string name)
        {
            this.connection = connection;
            this.name = name;
        }
    }
}
