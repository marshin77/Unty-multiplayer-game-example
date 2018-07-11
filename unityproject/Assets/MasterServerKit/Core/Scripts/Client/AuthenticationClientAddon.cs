// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;

using UnityEngine.Networking;

namespace MasterServerKit
{
    /// <summary>
    /// This client add-on is used for authenticating the player into the master server.
    /// </summary>
    public class AuthenticationClientAddon : ClientAddon
    {
        public Action onPlayerLoginSuccess;
        public Action<LoginError> onPlayerLoginError;
        public Action onPlayerRegistrationSuccess;
        public Action<RegistrationError> onPlayerRegistrationError;

        /// <summary>
        /// Registers the handlers for all the network messages this add-on is interested in.
        /// </summary>
        protected override void RegisterNetworkHandlers()
        {
            base.RegisterNetworkHandlers();
            networkClient.client.RegisterHandler(AuthenticationNetworkProtocol.ResponsePlayerLogin, OnResponsePlayerLogin);
            networkClient.client.RegisterHandler(AuthenticationNetworkProtocol.ResponsePlayerRegistration, OnResponsePlayerRegistration);
        }

        /// <summary>
        /// Unregisters the handlers for all the network messages this add-on is interested in.
        /// </summary>
        protected override void UnregisterNetworkHandlers()
        {
            networkClient.client.UnregisterHandler(AuthenticationNetworkProtocol.ResponsePlayerRegistration);
            networkClient.client.UnregisterHandler(AuthenticationNetworkProtocol.ResponsePlayerLogin);
            base.UnregisterNetworkHandlers();
        }

        /// <summary>
        /// Handler for the ResponsePlayerLogin message.
        /// </summary>
        /// <param name="netMsg">Network message that was received.</param>
        protected virtual void OnResponsePlayerLogin(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<ResponsePlayerLoginMessage>();
            if (msg != null)
            {
                if (msg.success)
                {
                    networkClient.username = msg.username;
                    if (onPlayerLoginSuccess != null)
                    {
                        onPlayerLoginSuccess.Invoke();
                        onPlayerLoginSuccess = null;
                    }
                }
                else
                {
                    if (onPlayerLoginError != null)
                    {
                        onPlayerLoginError(msg.error);
                        onPlayerLoginError = null;
                    }
                }
            }
        }

        /// <summary>
        /// Handler for the ResponsePlayerRegistration message.
        /// </summary>
        /// <param name="netMsg">Network message that was received.</param>
        protected virtual void OnResponsePlayerRegistration(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<ResponsePlayerRegistrationMessage>();
            if (msg != null)
            {
                if (msg.success)
                {
                    if (onPlayerRegistrationSuccess != null)
                    {
                        onPlayerRegistrationSuccess.Invoke();
                        onPlayerRegistrationSuccess = null;
                    }
                }
                else
                {
                    if (onPlayerRegistrationError != null)
                    {
                        onPlayerRegistrationError(msg.error);
                        onPlayerRegistrationError = null;
                    }
                }
            }
        }

        /// <summary>
        /// Registers a new user with the specified data in the system.
        /// </summary>
        /// <param name="email">The email address to register.</param>
        /// <param name="username">The username to register.</param>
        /// <param name="password">The password to register.</param>
        /// <param name="onSuccess">The callback to execute when the registration is successful.</param>
        /// <param name="onError">The callback to execute when the registration is not successful.</param>
        public void Register(string email, string username, string password,
            Action onSuccess, Action<RegistrationError> onError)
        {
            onPlayerRegistrationSuccess = onSuccess;
            onPlayerRegistrationError = onError;

            var msg = new RequestPlayerRegistrationMessage();
            msg.email = email;
            msg.username = username;
            msg.password = password;
            networkClient.client.Send(AuthenticationNetworkProtocol.RequestPlayerRegistration, msg);
        }

        /// <summary>
        /// Logs the specified user into the master server.
        /// </summary>
        /// <param name="username">The username to log in with.</param>
        /// <param name="password">The password to log in with.</param>
        /// <param name="onSuccess">The callback to execute when the login is successful.</param>
        /// <param name="onError">The callback to execute when the login is not successful.</param>
        public void Login(string username, string password, Action onSuccess, Action<LoginError> onError)
        {
            onPlayerLoginSuccess = onSuccess;
            onPlayerLoginError = onError;

            var msg = new RequestPlayerLoginMessage();
            msg.isAnonymous = false;
            msg.username = username;
            msg.password = password;
            networkClient.client.Send(AuthenticationNetworkProtocol.RequestPlayerLogin, msg);
        }

        /// <summary>
        /// Logs into the master server as a guest.
        /// </summary>
        /// <param name="onSuccess">The callback to execute when the login is successful.</param>
        /// <param name="onError">The callback to execute when the login is not successful.</param>
        public void LoginAsGuest(Action onSuccess, Action<LoginError> onError)
        {
            onPlayerLoginSuccess = onSuccess;
            onPlayerLoginError = onError;

            var msg = new RequestPlayerLoginMessage();
            msg.isAnonymous = true;
            networkClient.client.Send(AuthenticationNetworkProtocol.RequestPlayerLogin, msg);
        }
    }
}
