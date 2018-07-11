// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace MasterServerKit
{
    /// <summary>
    /// The master server is the entry point to the functionality available in Master Server Kit.
    /// It is responsible for managing the authentication of players and their requests for creating
    /// and joining games (in concert with the registered zone servers).
    /// </summary>
    public class MasterServer : BaseServer
    {
        /// <summary>
        /// The IP address of the master server.
        /// </summary>
        [Tooltip("The IP address of the master server.")]
        public string ip = "127.0.0.1";

        /// <summary>
        /// The port of the master server.
        /// </summary>
        [Tooltip("The port of the master server.")]
        public int port = 8000;

        /// <summary>
        /// True if guests are allowed in the master server; false otherwise.
        /// </summary>
        [Tooltip("True if guests are allowed in the master server; false otherwise.")]
        public bool allowGuests = true;

        /// <summary>
        /// True if there is a limit on how many players can be connected to the master server;
        /// false otherwise.
        /// </summary>
        [Tooltip("True if there is a limit on how many players can be connected to the master server; false otherwise.")]
        public bool playerLimit;

        /// <summary>
        /// The maximum number of players allowed on the master server.
        /// </summary>
        [Tooltip("The maximum number of players allowed on the master server.")]
        public int maxPlayers;

        /// <summary>
        /// The nickname to use with guest players.
        /// </summary>
        [Tooltip("The nickname to use with guest players.")]
        public string guestName;

        /// <summary>
        /// The host id of the WebGL server.
        /// </summary>
        public static int webGLHostId { get; protected set; }

        /// <summary>
        /// The server used to handle WebGL connections.
        /// </summary>
        protected NetworkServerSimple webGLServer = new NetworkServerSimple();

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        protected virtual void Awake()
        {
            Assert.IsTrue(!string.IsNullOrEmpty(ip));
        }

        /// <summary>
        /// Unity's Start method.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            webGLServer.SetNetworkConnectionClass<MasterServerWebGLCustomNetworkConnection>();
            webGLServer.useWebSockets = true;
            webGLServer.RegisterHandler(MsgType.Connect, OnWebGLConnected);
            webGLServer.RegisterHandler(MsgType.Disconnect, OnWebGLDisconnected);
            webGLServer.Listen(ip, port);
            webGLHostId = webGLServer.serverHostId;

            var config = new ConnectionConfig();
            config.AddChannel(QosType.ReliableSequenced);
            config.AddChannel(QosType.Unreliable);
            NetworkServer.Configure(config, maxPlayers > 0 ? maxPlayers : 1024);
            NetworkServer.useWebSockets = false;
            NetworkServer.Listen(ip, port);
        }

        /// <summary>
        /// Unity's Update method.
        /// </summary>
        protected virtual void Update()
        {
            webGLServer.Update();
        }

        /// <summary>
        /// Installs the default add-ons of this server. You can override this method in
        /// a subclass if you need a custom configuration.
        /// </summary>
        protected override void SetupDefaultAddons()
        {
            gameObject.AddComponent<AuthenticationServerAddon>();
            gameObject.AddComponent<GameRoomsMasterServerAddon>();
            gameObject.AddComponent<ChatServerAddon>();
        }

        /// <summary>
        /// Handler for the Connect message of WebGL clients.
        /// </summary>
        /// <param name="netMsg">The network message that was received.</param>
        protected virtual void OnWebGLConnected(NetworkMessage netMsg)
        {
            NetworkServer.AddExternalConnection(netMsg.conn);
        }

        /// <summary>
        /// Handler for the Disconnect message of WebGL clients.
        /// </summary>
        /// <param name="netMsg">The network message that was received.</param>
        protected virtual void OnWebGLDisconnected(NetworkMessage netMsg)
        {
            NetworkServer.RemoveExternalConnection(netMsg.conn.connectionId);
        }
    }
}