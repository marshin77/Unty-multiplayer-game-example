// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

namespace MasterServerKit
{
    /// <summary>
    /// Possible errors when a new player is registered in the system.
    /// </summary>
    public enum RegistrationError
    {
        DatabaseConnectionError,
        MissingEmailAddress,
        MissingUsername,
        MissingPassword,
        AlreadyExistingEmailAddress,
        AlreadyExistingUsername
    }

    /// <summary>
    /// Possible errors when an existing player logs into the system.
    /// </summary>
    public enum LoginError
    {
        DatabaseConnectionError,
        MissingUsername,
        MissingPassword,
        NonexistingUser,
        InvalidCredentials,
        ServerFull,
        AuthenticationRequired,
        UserAlreadyLoggedIn
    }

    /// <summary>
    /// Possible errors when creating a game room.
    /// </summary>
    public enum CreateGameRoomError
    {
        ZoneServerUnavailable
    }

    /// <summary>
    /// Possible errors when joining a game room.
    /// </summary>
    public enum JoinGameRoomError
    {
        GameFull,
        GameExpired,
        InvalidPassword
    }
}
