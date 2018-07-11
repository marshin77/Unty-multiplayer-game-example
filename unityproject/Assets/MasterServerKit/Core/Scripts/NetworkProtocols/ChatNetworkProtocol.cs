// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine.Networking;

namespace MasterServerKit
{
    /// <summary>
    /// The network protocol for the chat.
    /// </summary>
    public class ChatNetworkProtocol
    {
        public const short SendPublicChatText = 160;
        public const short SendPrivateChatText = 161;
    }

    public class SendPublicChatTextMessage : MessageBase
    {
        public string channel;
        public string sender;
        public string text;
    }

    public class SendPrivateChatTextMessage : MessageBase
    {
        public string channel;
        public string sender;
        public string recipient;
        public string text;
    }
}
