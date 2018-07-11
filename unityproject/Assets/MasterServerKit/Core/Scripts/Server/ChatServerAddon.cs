// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Networking;

namespace MasterServerKit
{
    /// <summary>
    /// Server add-on that provides chat functionality.
    /// </summary>
    [DisallowMultipleComponent]
    public class ChatServerAddon : ServerAddon
    {
        /// <summary>
        /// Registers the handlers for all the network messages this add-on is interested in.
        /// </summary>
        protected override void RegisterNetworkHandlers()
        {
            NetworkServer.RegisterHandler(ChatNetworkProtocol.SendPublicChatText, OnReceivedPublicChatText);
            NetworkServer.RegisterHandler(ChatNetworkProtocol.SendPrivateChatText, OnReceivedPrivateChatText);
        }

        /// <summary>
        /// Unregisters the handlers for all the network messages this add-on is interested in.
        /// </summary>
        protected override void UnregisterNetworkHandlers()
        {
            NetworkServer.UnregisterHandler(ChatNetworkProtocol.SendPrivateChatText);
            NetworkServer.UnregisterHandler(ChatNetworkProtocol.SendPublicChatText);
        }

        /// <summary>
        /// Handler for the SendPublicChatText message.
        /// </summary>
        /// <param name="netMsg">Network message that was received.</param>
        protected virtual void OnReceivedPublicChatText(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<SendPublicChatTextMessage>();
            var sender = baseServer.players.Find(x => x.connection == netMsg.conn);
            if (sender != null)
            {
                msg.sender = sender.name;
            }
            foreach (var player in baseServer.players)
            {
                player.connection.Send(ChatNetworkProtocol.SendPublicChatText, msg);
            }
        }

        /// <summary>
        /// Handler for the SendPrivateChatText message.
        /// </summary>
        /// <param name="netMsg">Network message that was received.</param>
        protected virtual void OnReceivedPrivateChatText(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<SendPrivateChatTextMessage>();
            var players = new List<Player>();
            var sender = baseServer.players.Find(x => x.connection == netMsg.conn);
            if (sender != null)
            {
                msg.sender = sender.name;
                players.Add(sender);
            }
            var recipient = baseServer.players.Find(x => x.name == msg.recipient);
            if (recipient != null)
            {
                players.Add(recipient);
            }
            foreach (var player in players)
            {
                player.connection.Send(ChatNetworkProtocol.SendPrivateChatText, msg);
            }
        }
    }
}
