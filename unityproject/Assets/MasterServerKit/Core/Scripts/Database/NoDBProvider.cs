// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections;
using System.Collections.Generic;

namespace MasterServerKit
{
    /// <summary>
    /// No database provider implementation. This is particularly useful for prototyping purposes,
    /// as there is no actual persistent storage but only in-memory storage during the lifetime of
    /// the application.
    /// </summary>
    public class NoDBProvider : IDatabaseProvider
    {
        /// <summary>
        /// The list of registered users.
        /// </summary>
        private List<User> users = new List<User>();

        /// <summary>
        /// Performs any initialization-related logic.
        /// </summary>
        public void InitializeDatabase()
        {
        }

        /// <summary>
        /// Registers a new user with the specified properties in the system.
        /// </summary>
        /// <param name="email">The new user's email address.</param>
        /// <param name="username">The new user's name.</param>
        /// <param name="password">The new user's password.</param>
        /// <param name="onSuccess">Delegate to be executed when the request is successful.</param>
        /// <param name="onError">Delegate to be executed when the request is not successful.</param>
        /// <returns>Async operation for the request.</returns>
        public IEnumerator Register(string email, string username, string password, Action<string> onSuccess, Action<RegistrationError> onError)
        {
            if (email.IsNullOrWhitespace())
            {
                onError(RegistrationError.MissingEmailAddress);
                yield break;
            }

            if (username.IsNullOrWhitespace())
            {
                onError(RegistrationError.MissingUsername);
                yield break;
            }

            if (password.IsNullOrWhitespace())
            {
                onError(RegistrationError.MissingPassword);
                yield break;
            }

            if (users.Find(x => x.Email == email) != null)
            {
                onError(RegistrationError.AlreadyExistingEmailAddress);
                yield break;
            }

            if (users.Find(x => x.Name == username) != null)
            {
                onError(RegistrationError.AlreadyExistingUsername);
                yield break;
            }

            var passwordToStore = CryptoUtils.GenerateCombinedSaltAndHash(password);

            var user = new User();
            user.Name = username;
            user.Email = email;
            user.Password = passwordToStore;
            user.IntProperties = new Dictionary<string, int>();
            user.StringProperties = new Dictionary<string, string>();
            users.Add(user);

            onSuccess(username);
        }

        /// <summary>
        /// Logs the specified user in the system.
        /// </summary>
        /// <param name="username">The user's name.</param>
        /// <param name="password">The user's password.</param>
        /// <param name="onSuccess">Delegate to be executed when the request is successful.</param>
        /// <param name="onError">Delegate to be executed when the request is not successful.</param>
        /// <returns>Async operation for the remote request.</returns>
        public IEnumerator Login(string username, string password, Action<string> onSuccess, Action<LoginError> onError)
        {
            if (username.IsNullOrWhitespace())
            {
                onError(LoginError.MissingUsername);
                yield break;
            }

            if (password.IsNullOrWhitespace())
            {
                onError(LoginError.MissingPassword);
                yield break;
            }

            var user = users.Find(x => x.Name == username);
            if (user == null)
            {
                onError(LoginError.NonexistingUser);
                yield break;
            }

            var storedPassword = user.Password;
            if (!CryptoUtils.CheckPasswordsAreEqual(password, storedPassword))
            {
                onError(LoginError.InvalidCredentials);
                yield break;
            }

            onSuccess(username);
        }

        /// <summary>
        /// Gets the specified integer property from the specified user.
        /// </summary>
        /// <param name="username">The user's name.</param>
        /// <param name="key">The property's key.</param>
        /// <param name="onSuccess">Delegate to be executed when the request is successful.</param>
        /// <param name="onError">Delegate to be executed when the request is not successful.</param>
        /// <returns>Async operation for the remote request.</returns>
        public IEnumerator GetIntProperty(string username, string key, Action<int> onSuccess, Action<string> onError)
        {
            var user = users.Find(x => x.Name == username);
            if (user == null)
            {
                onError("This user does not exist.");
                yield break;
            }

            if (!user.IntProperties.ContainsKey(key))
            {
                onError("This property does not exist.");
                yield break;
            }

            onSuccess(user.IntProperties[key]);
        }

        /// <summary>
        /// Sets the specified integer property from the specified user to the specified value.
        /// </summary>
        /// <param name="username">The user's name.</param>
        /// <param name="key">The property's key.</param>
        /// <param name="value">The property's value.</param>
        /// <param name="onSuccess">Delegate to be executed when the request is successful.</param>
        /// <param name="onError">Delegate to be executed when the request is not successful.</param>
        /// <returns>Async operation for the remote request.</returns>
        public IEnumerator SetIntProperty(string username, string key, int value, Action<int> onSuccess, Action<string> onError)
        {
            var user = users.Find(x => x.Name == username);
            if (user == null)
            {
                onError("This user does not exist.");
                yield break;
            }

            if (!user.IntProperties.ContainsKey(key))
            {
                user.IntProperties.Add(key, value);
            }
            else
            {
                user.IntProperties[key] = value;
            }

            onSuccess(value);
        }

        /// <summary>
        /// Gets the specified string property from the specified user.
        /// </summary>
        /// <param name="username">The user's name.</param>
        /// <param name="key">The property's key.</param>
        /// <param name="onSuccess">Delegate to be executed when the request is successful.</param>
        /// <param name="onError">Delegate to be executed when the request is not successful.</param>
        /// <returns>Async operation for the remote request.</returns>
        public IEnumerator GetStringProperty(string username, string key, Action<string> onSuccess, Action<string> onError)
        {
            var user = users.Find(x => x.Name == username);
            if (user == null)
            {
                onError("This user does not exist.");
                yield break;
            }

            if (!user.StringProperties.ContainsKey(key))
            {
                onError("This property does not exist.");
                yield break;
            }

            onSuccess(user.StringProperties[key]);
        }

        /// <summary>
        /// Sets the specified string property from the specified user to the specified value.
        /// </summary>
        /// <param name="username">The user's name.</param>
        /// <param name="key">The property's key.</param>
        /// <param name="value">The property's value.</param>
        /// <param name="onSuccess">Delegate to be executed when the request is successful.</param>
        /// <param name="onError">Delegate to be executed when the request is not successful.</param>
        /// <returns>Async operation for the remote request.</returns>
        public IEnumerator SetStringProperty(string username, string key, string value, Action<string> onSuccess, Action<string> onError)
        {
            var user = users.Find(x => x.Name == username);
            if (user == null)
            {
                onError("This user does not exist.");
                yield break;
            }

            if (!user.StringProperties.ContainsKey(key))
            {
                user.StringProperties.Add(key, value);
            }
            else
            {
                user.StringProperties[key] = value;
            }

            onSuccess(value);
        }
    }
}
