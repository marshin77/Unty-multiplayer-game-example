// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine.Networking;

namespace MasterServerKit
{
    /// <summary>
    /// The network protocol for the zone server.
    /// </summary>
    public class ZoneServerNetworkProtocol
    {
        public static readonly short RequestSpawnGameServer = 130;
        public static readonly short ResponseSpawnGameServer = 131;
    }

    public class RequestSpawnGameServerMessage : MessageBase
    {
        public string name;
        public int maxPlayers;
        public string password;
        public Property[] properties;
    }

    public class ResponseSpawnGameServerMessage : MessageBase
    {
        public int playerConnectionId;
        public bool success;
        public string ip;
        public int port;
    }
}