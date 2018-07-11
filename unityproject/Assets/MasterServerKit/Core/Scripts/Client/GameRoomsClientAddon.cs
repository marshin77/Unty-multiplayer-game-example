// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections.Generic;

using UnityEngine.Networking;

namespace MasterServerKit
{
    /// <summary>
    /// This client add-on is used for the game rooms functionality.
    /// </summary>
    public class GameRoomsClientAddon : ClientAddon
    {
        protected Action<int> pingCallback;
        public Action<SpawnedGameNetwork[]> findGameRoomsCallback;
        protected Action<string, int> createGameRoomSuccessCallback;
        protected Action<CreateGameRoomError> createGameRoomErrorCallback;
        protected Action<string, int> joinGameRoomSuccessCallback;
        protected Action<JoinGameRoomError> joinGameRoomErrorCallback;
        protected Action<string, int> playNowCallback;

        /// <summary>
        /// Registers the handlers for all the network messages this add-on is interested in.
        /// </summary>
        protected override void RegisterNetworkHandlers()
        {
            base.RegisterNetworkHandlers();
            networkClient.client.RegisterHandler(GameRoomsNetworkProtocol.ResponsePing, OnResponsePing);
            networkClient.client.RegisterHandler(GameRoomsNetworkProtocol.ResponseFindGameRooms, OnListGamesReceived);
            networkClient.client.RegisterHandler(GameRoomsNetworkProtocol.ResponseCreateGameRoom, OnResponseCreateRoom);
            networkClient.client.RegisterHandler(GameRoomsNetworkProtocol.ResponseJoinGameRoom, OnResponseJoinRoom);
        }

        /// <summary>
        /// Unregisters the handlers for all the network messages this add-on is interested in.
        /// </summary>
        protected override void UnregisterNetworkHandlers()
        {
            networkClient.client.UnregisterHandler(GameRoomsNetworkProtocol.ResponseJoinGameRoom);
            networkClient.client.UnregisterHandler(GameRoomsNetworkProtocol.ResponseCreateGameRoom);
            networkClient.client.UnregisterHandler(GameRoomsNetworkProtocol.ResponseFindGameRooms);
            networkClient.client.UnregisterHandler(GameRoomsNetworkProtocol.ResponsePing);
            base.UnregisterNetworkHandlers();
        }

        /// <summary>
        /// Pings the master server.
        /// </summary>
        /// <param name="callback">Callback to execute when a response is received.</param>
        public virtual void Ping(Action<int> callback)
        {
            pingCallback = callback;

            var msg = new RequestPingMessage();
            networkClient.client.Send(GameRoomsNetworkProtocol.RequestPing, msg);
        }

        /// <summary>
        /// Handler for the ResponsePing message.
        /// </summary>
        /// <param name="netMsg">Network message that was received.</param>
        protected virtual void OnResponsePing(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<ResponsePingMessage>();
            if (pingCallback != null)
            {
                pingCallback(msg.numConnectedPlayers);
            }
        }

        /// <summary>
        /// Requests the list of game rooms currently available in the master server.
        /// </summary>
        /// <param name="includeProperties">The returned game rooms will include these properties.</param>
        /// <param name="excludeProperties">The returned game rooms will not include these properties.</param>
        /// <param name="callback">Callback to execute when a response is received.</param>
        public virtual void FindGameRooms(List<Property> includeProperties, List<Property> excludeProperties,
            Action<SpawnedGameNetwork[]> callback)
        {
            findGameRoomsCallback = callback;

            var msg = new RequestFindGameRoomsMessage();
            msg.includeProperties = includeProperties.ToArray();
            msg.excludeProperties = excludeProperties.ToArray();
            networkClient.client.Send(GameRoomsNetworkProtocol.RequestFindGameRooms, msg);
        }

        /// <summary>
        /// Handler for the ResponseFindGameRooms message.
        /// </summary>
        /// <param name="netMsg">Network message that was received.</param>
        protected virtual void OnListGamesReceived(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<ResponseFindGameRoomsMessage>();
            if (msg != null && findGameRoomsCallback != null)
            {
                findGameRoomsCallback(msg.games);
            }
        }

        /// <summary>
        /// Requests the creation of a new game room with the specified properties.
        /// </summary>
        /// <param name="name">The name of this room.</param>
        /// <param name="maxPlayers">The maximum number of players allowed on this room.</param>
        /// <param name="password">Password (null for public rooms).</param>
        /// <param name="properties">The list of properties of this room.</param>
        /// <param name="successCallback">The callback to execute when the room could be created.</param>
        /// <param name="errorCallback">The callback to execute when the room could not be created.</param>
        public virtual void CreateGameRoom(string name, int maxPlayers, string password,
            List<Property> properties,
            Action<string, int> successCallback, Action<CreateGameRoomError> errorCallback)
        {
            createGameRoomSuccessCallback = successCallback;
            createGameRoomErrorCallback = errorCallback;

            var msg = new RequestCreateGameRoomMessage();
            msg.name = name;
            msg.maxPlayers = maxPlayers;
            msg.password = password;
            msg.properties = properties.ToArray();
            networkClient.client.Send(GameRoomsNetworkProtocol.RequestCreateGameRoom, msg);
        }

        /// <summary>
        /// Handler for the ResponseCreateGameRoom message.
        /// </summary>
        /// <param name="netMsg">Network message that was received.</param>
        protected virtual void OnResponseCreateRoom(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<ResponseCreateGameRoomMessage>();
            if (msg != null)
            {
                if (msg.success)
                {
                    if (createGameRoomSuccessCallback != null)
                    {
                        createGameRoomSuccessCallback(msg.ip, msg.port);
                        createGameRoomSuccessCallback = null;
                    }
                }
                else
                {
                    if (createGameRoomErrorCallback != null)
                    {
                        createGameRoomErrorCallback(msg.error);
                        createGameRoomErrorCallback = null;
                    }
                }
            }
        }

        /// <summary>
        /// Requests joining a game room with the specified properties.
        /// </summary>
        /// <param name="id">Id of the room to join.</param>
        /// <param name="password">Password of the room to join (can be null for public rooms).</param>
        /// <param name="successCallback">Callback to execute when the room could be joined.</param>
        /// <param name="errorCallback">Callback to execute when the room could not be joined.</param>
        public virtual void JoinGameRoom(int id, string password, Action<string, int> successCallback, Action<JoinGameRoomError> errorCallback)
        {
            joinGameRoomSuccessCallback = successCallback;
            joinGameRoomErrorCallback = errorCallback;

            var msg = new RequestJoinGameRoomMessage();
            msg.id = id;
            msg.password = password;
            networkClient.client.Send(GameRoomsNetworkProtocol.RequestJoinGameRoom, msg);
        }

        /// <summary>
        /// Handler for the ResponseJoinGameRoom message.
        /// </summary>
        /// <param name="netMsg">Network message that was received.</param>
        protected virtual void OnResponseJoinRoom(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<ResponseJoinGameRoomMessage>();
            if (msg.success)
            {
                if (joinGameRoomSuccessCallback != null)
                {
                    joinGameRoomSuccessCallback(msg.ip, msg.port);
                    joinGameRoomSuccessCallback = null;
                }
            }
            else
            {
                if (joinGameRoomErrorCallback != null)
                {
                    joinGameRoomErrorCallback(msg.error);
                    joinGameRoomErrorCallback = null;
                }
            }
        }

        /// <summary>
        /// Requests playing now. This will automatically join the first available game room or
        /// create a new one if none is available.
        /// </summary>
        /// <param name="joinCallback">Callback to execute when an available room is joined.</param>
        /// <param name="createSuccessCallback">Callback to execute when a new room is created.</param>
        /// <param name="createErrorCallback">Callback to execute when a new room could not be created.</param>
        public virtual void PlayNow(Action<string, int> joinCallback,
            Action<string, int> createSuccessCallback, Action<CreateGameRoomError> createErrorCallback)
        {
            joinGameRoomSuccessCallback = joinCallback;
            createGameRoomSuccessCallback = createSuccessCallback;
            createGameRoomErrorCallback = createErrorCallback;

            var msg = new RequestPlayNowMessage();
            networkClient.client.Send(GameRoomsNetworkProtocol.RequestPlayNow, msg);
        }
    }
}