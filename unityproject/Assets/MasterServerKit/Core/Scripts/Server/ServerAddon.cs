// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Networking;

namespace MasterServerKit
{
    /// <summary>
    /// A server add-on is responsible for an specific piece of functionality that we are interested
    /// in for a server (i.e., registering and handling a set of network handlers). They allow us to
    /// keep different features (e.g, authentication, game rooms, chat) in different classes, instead
    /// of having every single feature crammed into the server itself.
    /// </summary>
    public class ServerAddon : MonoBehaviour
    {
        /// <summary>
        /// The underlying base server.
        /// </summary>
        protected BaseServer baseServer;

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        public virtual void Awake()
        {
            baseServer = GetComponent<BaseServer>();
        }

        /// <summary>
        /// Unity's OnEnable method.
        /// </summary>
        public virtual void OnEnable()
        {
            RegisterNetworkHandlers();
        }

        /// <summary>
        /// Unity's OnDisable method.
        /// </summary>
        public virtual void OnDisable()
        {
            UnregisterNetworkHandlers();
        }

        /// <summary>
        /// Called when a player is disconnected from the base server.
        /// </summary>
        /// <param name="connection">The network connection of the disconnected player.</param>
        public virtual void OnDisconnectedFromBaseServer(NetworkConnection connection)
        {
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
