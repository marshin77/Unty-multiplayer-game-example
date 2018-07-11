// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine.Networking;

namespace MasterServerKit
{
    /// <summary>
    /// The network protocol for the base server.
    /// </summary>
    public class BaseServerNetworkProtocol
    {
        public static readonly short SendPlayerData = 150;
    }

    public class SendPlayerDataMessage : MessageBase
    {
        public string username;
    }
}
