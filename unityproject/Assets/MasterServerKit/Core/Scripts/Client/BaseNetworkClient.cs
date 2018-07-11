// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;

using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

namespace MasterServerKit
{
    /// <summary>
    /// Useful base class for all the network clients that represent a player used in the kit.
    /// </summary>
    public class BaseNetworkClient
    {
        /// <summary>
        /// The underlying network client.
        /// </summary>
        public NetworkClient client { get; protected set; }

        /// <summary>
        /// The username of the player.
        /// </summary>
        public string username;

        /// <summary>
        /// Callback to execute when this client connects to a server.
        /// </summary>
        public Action onConnected;

        /// <summary>
        /// Callback to execute when this client is disconnected from a server.
        /// </summary>
        public Action onDisconnected;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="client">Network client.</param>
        /// <param name="username">Username.</param>
        public BaseNetworkClient(NetworkClient client, string username)
        {
            this.client = client;
            this.username = username;
            RegisterHandlers();
        }

        /// <summary>
        /// Registers the handlers for all the network messages this client is
        /// interested in.
        /// </summary>
        protected virtual void RegisterHandlers()
        {
            client.RegisterHandler(MsgType.Connect, OnConnected);
            client.RegisterHandler(MsgType.Disconnect, OnDisconnected);
            client.RegisterHandler(MsgType.Error, OnError);
        }

        /// <summary>
        /// Handler for the Connect message.
        /// </summary>
        /// <param name="netMsg">Network message that was received.</param>
        protected void OnConnected(NetworkMessage netMsg)
        {
            OnConnected();
            if (onConnected != null)
            {
                onConnected();
            }
        }

        /// <summary>
        /// Handler for the Disconnect message.
        /// </summary>
        /// <param name="netMsg">Network message that was received.</param>
        protected void OnDisconnected(NetworkMessage netMsg)
        {
            OnDisconnected();
            if (onDisconnected != null)
            {
                onDisconnected();
            }
        }

        /// <summary>
        /// Handler for the Error message.
        /// </summary>
        /// <param name="netMsg">Network message that was received.</param>
        protected void OnError(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<ErrorMessage>();
            OnError(msg.errorCode);
        }

        /// <summary>
        /// This method is called when this client is connected to a server.
        /// It is intended to be overriden by subclasses as appropriate.
        /// </summary>
        protected virtual void OnConnected()
        {
        }

        /// <summary>
        /// This method is called when this client is disconnected from a server.
        /// It is intended to be overriden by subclasses as appropriate.
        /// </summary>
        protected virtual void OnDisconnected()
        {
        }

        /// <summary>
        /// This method is called when an error occurs on this client.
        /// It is intended to be overriden by subclasses as appropriate.
        /// </summary>
        /// <param name="errorCode">Error code.</param>
        protected virtual void OnError(int errorCode)
        {
        }
    }
}
