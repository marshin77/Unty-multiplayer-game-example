// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

namespace MasterServerKit
{
    /// <summary>
    /// Available types of client add-ons.
    /// </summary>
    public enum ClientAddonType
    {
        MasterServer,
        Game
    }

    /// <summary>
    /// A client add-on is responsible for an specific piece of functionality that we are interested
    /// in for a client (i.e., registering and handling a set of network handlers). They allow us to
    /// keep different features (e.g, authentication, game rooms, chat) in different classes, instead
    /// of having every single feature crammed into the client itself.
    /// </summary>
    [RequireComponent(typeof(ClientAPI))]
    public class ClientAddon : MonoBehaviour
    {
        /// <summary>
        /// The type of this client add-on.
        /// </summary>
        [HideInInspector]
        public ClientAddonType type;

        /// <summary>
        /// The underlying network client.
        /// </summary>
        protected BaseNetworkClient networkClient;

        /// <summary>
        /// Registers this add-on with the specified network client.
        /// </summary>
        /// <param name="type">The type of client with which to register this add-on.</param>
        public virtual void RegisterAddon(ClientAddonType type)
        {
            this.type = type;
            switch (type)
            {
                case ClientAddonType.MasterServer:
                    networkClient = ClientAPI.masterServerClient;
                    break;

                case ClientAddonType.Game:
                    networkClient = ClientAPI.gameClient;
                    break;
            }

            RegisterNetworkHandlers();
        }

        /// <summary>
        /// Unregisters this add-on from the current network client.
        /// </summary>
        public virtual void UnregisterAddon()
        {
            UnregisterNetworkHandlers();
        }

        /// <summary>
        /// Registers the handlers for all the network messages this add-on is interested in.
        /// </summary>
        protected virtual void RegisterNetworkHandlers()
        {
        }

        /// <summary>
        /// Unregisters the handlers for all the network messages this add-on is interested in.
        /// </summary>
        protected virtual void UnregisterNetworkHandlers()
        {
        }
    }
}
