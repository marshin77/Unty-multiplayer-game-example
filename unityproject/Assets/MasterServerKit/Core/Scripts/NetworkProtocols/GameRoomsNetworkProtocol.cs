// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine.Networking;

namespace MasterServerKit
{
    /// <summary>
    /// The network protocol for the game rooms.
    /// </summary>
    public class GameRoomsNetworkProtocol
    {
        public static readonly short RequestPing = 110;
        public static readonly short ResponsePing = 111;
        public static readonly short RegisterZoneServer = 112;
        public static readonly short RegisterGameServer = 113;
        public static readonly short UnregisterGameServer = 114;
        public static readonly short UnregisterGameServerResponse = 115;
        public static readonly short RequestFindGameRooms = 116;
        public static readonly short ResponseFindGameRooms = 117;
        public static readonly short RequestCreateGameRoom = 118;
        public static readonly short ResponseCreateGameRoom = 119;
        public static readonly short RequestDestroyGameRoom = 120;
        public static readonly short ResponseDestroyGameRoom = 121;
        public static readonly short RequestJoinGameRoom = 122;
        public static readonly short ResponseJoinGameRoom = 123;
        public static readonly short RequestPlayNow = 124;
        public static readonly short UpdateGameServerState = 125;
        public static readonly short NotifyGameServerReady = 126;
    }

    public class RequestPingMessage : MessageBase
    {
    }

    public class ResponsePingMessage : MessageBase
    {
        public int numConnectedPlayers;
    }

    public class RegisterZoneServerMessage : MessageBase
    {
        public string ip;
        public int port;
    }

    public class RegisterGameServerMessage : MessageBase
    {
        public string ip;
        public int port;
        public string name;
        public int maxPlayers;
        public string password;
        public bool hideWhenFull;
        public Property[] properties;
    }

    public class UnregisterGameServerMessage : MessageBase
    {
    }

    public class UnregisterGameServerResponseMessage : MessageBase
    {
    }

    public class RequestFindGameRoomsMessage : MessageBase
    {
        public Property[] includeProperties;
        public Property[] excludeProperties;
    }

    public class ResponseFindGameRoomsMessage : MessageBase
    {
        public SpawnedGameNetwork[] games;
    }

    public class RequestCreateGameRoomMessage : MessageBase
    {
        public int playerConnectionId;
        public string name;
        public int maxPlayers;
        public string password;
        public Property[] properties;
    }

    public class ResponseCreateGameRoomMessage : MessageBase
    {
        public int playerConnectionId;
        public bool success;
        public CreateGameRoomError error;
        public string ip;
        public int port;
    }

    public class RequestDestroyGameRoomMessage : MessageBase
    {
        public int port;
    }

    public class ResponseDestroyGameRoomMessage : MessageBase
    {
    }

    public class RequestJoinGameRoomMessage : MessageBase
    {
        public int id;
        public string password;
    }

    public class ResponseJoinGameRoomMessage : MessageBase
    {
        public bool success;
        public JoinGameRoomError error;
        public string ip;
        public int port;
    }

    public class RequestPlayNowMessage : MessageBase
    {
    }

    public class UpdateGameServerStateMessage : MessageBase
    {
        public int numPlayers;
    }

    public class NotifyGameServerReadyMessage : MessageBase
    {
        public int playerConnectionId;
        public string ip;
        public int port;
    }
}