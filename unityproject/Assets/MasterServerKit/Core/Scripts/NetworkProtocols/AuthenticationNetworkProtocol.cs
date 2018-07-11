// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine.Networking;

namespace MasterServerKit
{
    /// <summary>
    /// The network protocol for the authentication process.
    /// </summary>
    public class AuthenticationNetworkProtocol
    {
        public static readonly short RequestPlayerLogin = 100;
        public static readonly short ResponsePlayerLogin = 101;
        public static readonly short RequestPlayerRegistration = 102;
        public static readonly short ResponsePlayerRegistration = 103;
    }

    public class RequestPlayerLoginMessage : MessageBase
    {
        public bool isAnonymous;
        public string username;
        public string password;
    }

    public class ResponsePlayerLoginMessage : MessageBase
    {
        public bool success;
        public LoginError error;
        public string username;
    }

    public class RequestPlayerRegistrationMessage : MessageBase
    {
        public string email;
        public string username;
        public string password;
    }

    public class ResponsePlayerRegistrationMessage : MessageBase
    {
        public bool success;
        public RegistrationError error;
    }
}
