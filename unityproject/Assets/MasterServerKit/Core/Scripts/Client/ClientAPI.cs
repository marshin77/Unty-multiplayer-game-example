// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Networking;

namespace MasterServerKit
{
    /// <summary>
    /// This class is the client-side entry point to all of the functionality available in
    /// Master Server Kit.
    /// </summary>
    public class ClientAPI : MonoBehaviour
    {
        /// <summary>
        /// The IP address of the master server.
        /// </summary>
        [Tooltip("The IP address of the master server.")]
        public string masterServerIp;

        /// <summary>
        /// The port of the master server.
        /// </summary>
        [Tooltip("The port of the master server.")]
        public int masterServerPort;

        /// <summary>
        /// True if you are using the Network Manager component available in UNET; false otherwise.
        /// </summary>
        [Tooltip("True if you are using the Network Manager component available in UNET; false otherwise.")]
        public bool useNetworkManagerPublic;

        /// <summary>
        /// True if you are using the Network Manager component available in UNET; false otherwise.
        /// </summary>
        public static bool useNetworkManager
        {
            get { return instance.useNetworkManagerPublic; }
        }

        /// <summary>
        /// Cached static instance.
        /// </summary>
        private static ClientAPI instance;

        /// <summary>
        /// The network client used to connect to the master server.
        /// </summary>
        public static BaseNetworkClient masterServerClient { get; private set; }

        /// <summary>
        /// The network client used to connect to the game server.
        /// </summary>
        public static GameClient gameClient;

        /// <summary>
        /// Unity's Awake method.
        /// </summary>
        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Unity's Start method.
        /// </summary>
        protected virtual void Start()
        {
            masterServerClient = new BaseNetworkClient(new NetworkClient(), "Anonymous");
            SetupDefaultMasterServerClientAddons();
        }

        /// <summary>
        /// Installs the default add-ons of the master server client. You can override this method in
        /// a subclass if you need a custom configuration.
        /// </summary>
        protected virtual void SetupDefaultMasterServerClientAddons()
        {
            var authAddon = gameObject.AddComponent<AuthenticationClientAddon>();
            authAddon.RegisterAddon(ClientAddonType.MasterServer);
            var gameRoomsAddon = gameObject.AddComponent<GameRoomsClientAddon>();
            gameRoomsAddon.RegisterAddon(ClientAddonType.MasterServer);
            var chatAddon = gameObject.AddComponent<ChatClientAddon>();
            chatAddon.RegisterAddon(ClientAddonType.MasterServer);
        }

        /// <summary>
        /// Installs the default add-ons of the game client. You can override this method in
        /// a subclass if you need a custom configuration.
        /// </summary>
        protected virtual void SetupDefaultGameClientAddons()
        {
            var addonsToRemove = new List<ClientAddon>();
            foreach (var addon in GetComponents<ChatClientAddon>())
            {
                if (addon.type == ClientAddonType.Game)
                {
                    addonsToRemove.Add(addon);
                }
            }
            foreach (var addon in addonsToRemove)
            {
                Destroy(addon);
            }
            var chatAddon = gameObject.AddComponent<ChatClientAddon>();
            chatAddon.RegisterAddon(ClientAddonType.Game);
        }

        /// <summary>
        /// Connects to the master server.
        /// </summary>
        /// <param name="onConnected">The callback to execute when a connection to the master server is
        /// successfully established.</param>
        /// <param name="onDisconnected">The callback to execute when a connection to the master server
        /// cannot be successfully established.</param>
        public static void ConnectToMasterServer(Action onConnected, Action onDisconnected)
        {
            masterServerClient.onConnected = onConnected;
            masterServerClient.onDisconnected = onDisconnected;
            masterServerClient.client.Connect(instance.masterServerIp, instance.masterServerPort);
        }

        /// <summary>
        /// Registers a new user with the specified data in the system.
        /// </summary>
        /// <param name="email">The email address to register.</param>
        /// <param name="username">The username to register.</param>
        /// <param name="password">The password to register.</param>
        /// <param name="onSuccess">The callback to execute when the registration is successful.</param>
        /// <param name="onError">The callback to execute when the registration is not successful.</param>
        public static void Register(
            string email, string username, string password,
            Action onSuccess, Action<RegistrationError> onError)
        {
            var authAddon = GetMasterServerClientAddon<AuthenticationClientAddon>();
            if (authAddon != null)
            {
                authAddon.Register(email, username, password, onSuccess, onError);
            }
        }

        /// <summary>
        /// Logs the specified user into the master server.
        /// </summary>
        /// <param name="username">The username to log in with.</param>
        /// <param name="password">The password to log in with.</param>
        /// <param name="onSuccess">The callback to execute when the login is successful.</param>
        /// <param name="onError">The callback to execute when the login is not successful.</param>
        public static void Login(
            string username, string password,
            Action onSuccess, Action<LoginError> onError)
        {
            var authAddon = GetMasterServerClientAddon<AuthenticationClientAddon>();
            if (authAddon != null)
            {
                authAddon.Login(username, password, onSuccess, onError);
            }
        }

        /// <summary>
        /// Logs into the master server as a guest.
        /// </summary>
        /// <param name="onSuccess">The callback to execute when the login is successful.</param>
        /// <param name="onError">The callback to execute when the login is not successful.</param>
        public static void LoginAsGuest(
            Action onSuccess, Action<LoginError> onError)
        {
            var authAddon = GetMasterServerClientAddon<AuthenticationClientAddon>();
            if (authAddon != null)
            {
                authAddon.LoginAsGuest(onSuccess, onError);
            }
        }

        /// <summary>
        /// Pings the master server.
        /// </summary>
        /// <param name="callback">The callback to execute when a response is received.</param>
        public static void Ping(Action<int> callback)
        {
            var gameRoomsAddon = GetMasterServerClientAddon<GameRoomsClientAddon>();
            if (gameRoomsAddon != null)
            {
                gameRoomsAddon.Ping(callback);
            }
        }

        /// <summary>
        /// Requests the list of game rooms currently available in the master server.
        /// </summary>
        /// <param name="includeProperties">The returned game rooms will include these properties.</param>
        /// <param name="excludeProperties">The returned game rooms will not include these properties.</param>
        /// <param name="callback">The callback to execute when a response is received.</param>
        public static void FindGameRooms(List<Property> includeProperties, List<Property> excludeProperties,
            Action<SpawnedGameNetwork[]> callback)
        {
            var gameRoomsAddon = GetMasterServerClientAddon<GameRoomsClientAddon>();
            if (gameRoomsAddon != null)
            {
                gameRoomsAddon.FindGameRooms(includeProperties, excludeProperties, callback);
            }
        }

        /// <summary>
        /// Requests the creation of a new game room in the master server.
        /// </summary>
        /// <param name="name">The name of this room.</param>
        /// <param name="maxPlayers">The maximum number of players allowed in this room.</param>
        /// <param name="password">The password of this room (null for public rooms).</param>
        /// <param name="successCallback">The callback to execute when the room could be created.</param>
        /// <param name="errorCallback">The callback to execute when the room could not be created.</param>
        public static void CreateGameRoom(string name, int maxPlayers, string password,
            Action<string, int> successCallback, Action<CreateGameRoomError> errorCallback)
        {
            var gameRoomsAddon = GetMasterServerClientAddon<GameRoomsClientAddon>();
            if (gameRoomsAddon != null)
            {
                gameRoomsAddon.CreateGameRoom(name, maxPlayers, password, new List<Property>(), successCallback, errorCallback);
            }
        }

        /// <summary>
        /// Requests the creation of a new game room with the specified properties in the master server.
        /// </summary>
        /// <param name="name">The name of this room.</param>
        /// <param name="maxPlayers">The maximum number of players allowed in this room.</param>
        /// <param name="password">The password of this room (null for public rooms).</param>
        /// <param name="properties">The properties of this room.</param>
        /// <param name="successCallback">The callback to execute when the room could be created.</param>
        /// <param name="errorCallback">The callback to execute when the room could not be created.</param>
        public static void CreateGameRoom(string name, int maxPlayers, string password,
            List<Property> properties,
            Action<string, int> successCallback, Action<CreateGameRoomError> errorCallback)
        {
            var gameRoomsAddon = GetMasterServerClientAddon<GameRoomsClientAddon>();
            if (gameRoomsAddon != null)
            {
                gameRoomsAddon.CreateGameRoom(name, maxPlayers, password, properties, successCallback, errorCallback);
            }
        }

        /// <summary>
        /// Requests joining a game room with the specified properties in the master server.
        /// </summary>
        /// <param name="roomId">The id of the room to join.</param>
        /// <param name="password">The password of the room to join (null for public rooms).</param>
        /// <param name="successCallback">The callback to execute when the room could be joined.</param>
        /// <param name="errorCallback">The callback to execute when the room could not be joined.</param>
        public static void JoinGameRoom(int roomId, string password,
            Action<string, int> successCallback, Action<JoinGameRoomError> errorCallback)
        {
            var gameRoomsAddon = GetMasterServerClientAddon<GameRoomsClientAddon>();
            if (gameRoomsAddon != null)
            {
                gameRoomsAddon.JoinGameRoom(roomId, password, successCallback, errorCallback);
            }
        }

        /// <summary>
        /// Requests playing now in the master server. This will automatically join the first available
        /// room or create a new one if none is available.
        /// </summary>
        /// <param name="joinCallback">The callback to execute when an available room is joined.</param>
        /// <param name="createSuccessCallback">Callback to execute when a new room is created.</param>
        /// <param name="createErrorCallback">Callback to execute when a new room could not be created.</param>
        public static void PlayNow(Action<string, int> joinCallback,
            Action<string, int> createSuccessCallback, Action<CreateGameRoomError> createErrorCallback)
        {
            var gameRoomsAddon = GetMasterServerClientAddon<GameRoomsClientAddon>();
            if (gameRoomsAddon != null)
            {
                gameRoomsAddon.PlayNow(joinCallback, createSuccessCallback, createErrorCallback);
            }
        }

        /// <summary>
        /// Creates a new game client connected to the specified IP address and port.
        /// </summary>
        /// <param name="ip">The IP address of the game server.</param>
        /// <param name="port">The port of the game server.</param>
        /// <param name="onConnected">The callback to execute when the game server is joined.</param>
        public static void JoinGameServer(string ip, int port, Action onConnected)
        {
            if (gameClient != null && gameClient.client.isConnected)
            {
                gameClient.client.Disconnect();
                gameClient = null;
            }
            var client = new GameClient(new NetworkClient(), masterServerClient.username);
            client.onConnected = onConnected;
            gameClient = client;
            instance.SetupDefaultGameClientAddons();
            gameClient.client.Connect(ip, port);
        }

        /// <summary>
        /// Creates a new game client connected to the specified IP address and port using the
        /// Network Manager.
        /// </summary>
        /// <param name="ip">The IP address of the game server.</param>
        /// <param name="port">The port of the game server.</param>
        public static void JoinGameServer(string ip, int port)
        {
            NetworkManager.singleton.networkAddress = ip;
            NetworkManager.singleton.networkPort = port;
            if (gameClient != null && gameClient.client.isConnected)
            {
                gameClient.client.Disconnect();
                gameClient = null;
            }
            var client = new GameClient(NetworkManager.singleton.StartClient(), masterServerClient.username);
            gameClient = client;
            instance.SetupDefaultGameClientAddons();
        }

        /// <summary>
        /// Gets the add-on with type T installed on this client.
        /// </summary>
        /// <typeparam name="T">Type of the add-on to return.</typeparam>
        /// <returns>The add-on with type T installed on this client.</returns>
        public static T GetMasterServerClientAddon<T>() where T : ClientAddon
        {
            var addons = instance.GetComponents<T>();
            var addon = Array.Find(addons, x => x.type == ClientAddonType.MasterServer);
            if (addon != null)
            {
                return addon as T;
            }
            return null;
        }

        /// <summary>
        /// Gets the add-on with type T installed on this client.
        /// </summary>
        /// <typeparam name="T">Type of the add-on to return.</typeparam>
        /// <returns>The add-on with type T installed on this client.</returns>
        public static T GetGameClientAddon<T>() where T : ClientAddon
        {
            var addons = instance.GetComponents<T>();
            var addon = Array.Find(addons, x => x.type == ClientAddonType.Game);
            if (addon != null)
            {
                return addon as T;
            }
            return null;
        }
    }
}