// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Networking;

namespace MasterServerKit
{
    /// <summary>
    /// Server add-on that provides player authentication functionality.
    /// </summary>
    [DisallowMultipleComponent]
    public class AuthenticationServerAddon : ServerAddon
    {
        /// <summary>
        /// Registers the handlers for all the network messages this plugin is
        /// interested in.
        /// </summary>
        protected override void RegisterNetworkHandlers()
        {
            NetworkServer.RegisterHandler(AuthenticationNetworkProtocol.RequestPlayerLogin, OnPlayerLoginRequested);
            NetworkServer.RegisterHandler(AuthenticationNetworkProtocol.RequestPlayerRegistration, OnPlayerRegistrationRequested);
        }

        /// <summary>
        /// Unregisters the handlers for all the network messages this plugin is
        /// interested in.
        /// </summary>
        protected override void UnregisterNetworkHandlers()
        {
            NetworkServer.UnregisterHandler(AuthenticationNetworkProtocol.RequestPlayerRegistration);
            NetworkServer.UnregisterHandler(AuthenticationNetworkProtocol.RequestPlayerLogin);
        }

        /// <summary>
        /// Handler for the RequestPlayerLogin message.
        /// </summary>
        /// <param name="netMsg">Network message that was received.</param>
        protected virtual void OnPlayerLoginRequested(NetworkMessage netMsg)
        {
            var masterServer = baseServer as MasterServer;

            var msg = netMsg.ReadMessage<RequestPlayerLoginMessage>();

            var responseMsg = IsJoinAllowed(msg);
            if (responseMsg.success == false)
            {
                netMsg.conn.Send(AuthenticationNetworkProtocol.ResponsePlayerLogin, responseMsg);
                return;
            }

            if (msg.isAnonymous)
            {
                var player = new Player(netMsg.conn, masterServer.guestName);
                baseServer.players.Add(player);

                responseMsg.success = true;
                responseMsg.username = player.name;
                netMsg.conn.Send(AuthenticationNetworkProtocol.ResponsePlayerLogin, responseMsg);
            }
            else
            {
                StartCoroutine(DatabaseService.Login(msg.username, msg.password, username =>
                {
                    responseMsg.success = true;
                    responseMsg.username = msg.username;
                    netMsg.conn.Send(AuthenticationNetworkProtocol.ResponsePlayerLogin, responseMsg);

                    var player = new Player(netMsg.conn, msg.username);
                    baseServer.players.Add(player);
                },
                error =>
                {
                    responseMsg.success = false;
                    responseMsg.error = error;
                    netMsg.conn.Send(AuthenticationNetworkProtocol.ResponsePlayerLogin, responseMsg);
                }));
            }
        }

        /// <summary>
        /// Checks if a new login into the master server is allowed.
        /// </summary>
        /// <param name="msg">Login information to check.</param>
        /// <returns>The response to send to the player trying to join the master server.</returns>
        protected virtual ResponsePlayerLoginMessage IsJoinAllowed(RequestPlayerLoginMessage msg)
        {
            var masterServer = baseServer as MasterServer;

            var responseMsg = new ResponsePlayerLoginMessage();
            if (!msg.isAnonymous && baseServer.players.Find(x => x.name == msg.username) != null)
            {
                responseMsg.success = false;
                responseMsg.error = LoginError.UserAlreadyLoggedIn;
            }
            else if (masterServer.playerLimit && baseServer.players.Count >= masterServer.maxPlayers)
            {
                responseMsg.success = false;
                responseMsg.error = LoginError.ServerFull;
            }
            else
            {
                if (msg.isAnonymous)
                {
                    if (!masterServer.allowGuests)
                    {
                        responseMsg.success = false;
                        responseMsg.error = LoginError.AuthenticationRequired;
                    }
                    else
                    {
                        responseMsg.success = true;
                    }
                }
                else
                {
                    responseMsg.success = true;
                }
            }

            return responseMsg;
        }

        /// <summary>
        /// Handler for the RequestPlayerRegistration message.
        /// </summary>
        /// <param name="netMsg">Network message that was received.</param>
        protected virtual void OnPlayerRegistrationRequested(NetworkMessage netMsg)
        {
            var msg = netMsg.ReadMessage<RequestPlayerRegistrationMessage>();
            StartCoroutine(DatabaseService.Register(msg.email, msg.username, msg.password, token =>
            {
                // Send the success response to the player.
                var responseMsg = new ResponsePlayerRegistrationMessage();
                responseMsg.success = true;
                netMsg.conn.Send(AuthenticationNetworkProtocol.ResponsePlayerRegistration, responseMsg);
            },
            error =>
            {
                // Send the error response to the player.
                var responseMsg = new ResponsePlayerRegistrationMessage();
                responseMsg.success = false;
                responseMsg.error = error;
                netMsg.conn.Send(AuthenticationNetworkProtocol.ResponsePlayerRegistration, responseMsg);
            }));
        }
    }
}
