// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;

using UnityEngine.Networking;

namespace MasterServerKit
{
    /// <summary>
    /// This client add-on is used for the chat functionality.
    /// </summary>
    public class ChatClientAddon : ClientAddon
    {
        public Action<string, string, string> onReceivedPublicChatText;
        public Action<string, string, string, string> onReceivedPrivateChatText;

        /// <summary>
        /// Registers the handlers for all the network messages this add-on is interested in.
        /// </summary>
        protected override void RegisterNetworkHandlers()
        {
            base.RegisterNetworkHandlers();
            networkClient.client.RegisterHandler(ChatNetworkProtocol.SendPublicChatText, OnReceivedPublicChatText);
            networkClient.client.RegisterHandler(ChatNetworkProtocol.SendPrivateChatText, OnReceivedPrivateChatText);
        }

        /// <summary>
        /// Unregisters the handlers for all the network messages this add-on is interested in.
        /// </summary>
        protected override void UnregisterNetworkHandlers()
        {
            networkClient.client.UnregisterHandler(ChatNetworkProtocol.SendPrivateChatText);
            networkClient.client.UnregisterHandler(ChatNetworkProtocol.SendPublicChatText);
            base.UnregisterNetworkHandlers();
        }

        /// <summary>
        /// Handler for the SendPublicChatText message.
        /// </summary>
        /// <param name="netMsg">Network message that was received.</param>
        protected virtual void OnReceivedPublicChatText(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<SendPublicChatTextMessage>();
            if (msg != null)
            {
                if (onReceivedPublicChatText != null)
                {
                    onReceivedPublicChatText(msg.channel, msg.sender, msg.text);
                }
            }
        }

        /// <summary>
        /// Handler for the SendPrivateChatText message.
        /// </summary>
        /// <param name="netMsg">Network message that was received.</param>
        protected virtual void OnReceivedPrivateChatText(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<SendPrivateChatTextMessage>();
            if (msg != null)
            {
                if (onReceivedPrivateChatText != null)
                {
                    onReceivedPrivateChatText(msg.channel, msg.sender, msg.recipient, msg.text);
                }
            }
        }

        /// <summary>
        /// Sends the specified public text message to the specified chat channel.
        /// </summary>
        /// <param name="channel">Channel to which to send the specified text to.</param>
        /// <param name="text">Text to send.</param>
        public virtual void SendPublicChatMessage(string channel, string text)
        {
            var msg = new SendPublicChatTextMessage();
            msg.channel = channel;
            msg.text = text;
            networkClient.client.Send(ChatNetworkProtocol.SendPublicChatText, msg);
        }

        /// <summary>
        /// Sends the specified private text message to the specified recipient and the specified
        /// chat channel.
        /// </summary>
        /// <param name="channel">Channel to which to send the specified text to.</param>
        /// <param name="recipient">Name of the user to which to send the specified text to.</param>
        /// <param name="text">Text to send.</param>
        public virtual void SendPrivateChatMessage(string channel, string recipient, string text)
        {
            var msg = new SendPrivateChatTextMessage();
            msg.channel = channel;
            msg.recipient = recipient;
            msg.text = text;
            networkClient.client.Send(ChatNetworkProtocol.SendPrivateChatText, msg);
        }
    }
}
