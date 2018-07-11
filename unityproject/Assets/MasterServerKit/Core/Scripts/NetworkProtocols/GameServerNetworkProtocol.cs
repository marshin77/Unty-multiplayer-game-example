// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine.Networking;

namespace MasterServerKit
{
    /// <summary>
    /// The network protocol for the game server.
    /// </summary>
    public class GameServerNetworkProtocol
    {
        public static readonly short RequestGameServerRegistration = 140;
        public static readonly short ResponseGameServerRegistration = 141;
    }

    public class RequestGameServerRegistrationMessage : MessageBase
    {
        public int playerConnectionId;
        public string zoneServerIp;
        public int zoneServerPort;
        public string name;
        public int maxPlayers;
        public string password;
        public Property[] properties;
    }

    public class ResponseGameServerRegistrationMessage : MessageBase
    {
    }
}