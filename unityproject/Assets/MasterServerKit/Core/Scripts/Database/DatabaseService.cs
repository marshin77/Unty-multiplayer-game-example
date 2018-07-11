// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections;

namespace MasterServerKit
{
    /// <summary>
    /// Utility class to communicate with the selected database provider.
    /// </summary>
    public static class DatabaseService
    {
        /// <summary>
        /// Default database provider.
        /// </summary>
        private static IDatabaseProvider database = new SQLiteProvider();

        /// <summary>
        /// Constructor.
        /// </summary>
        static DatabaseService()
        {
            database.InitializeDatabase();
        }

        /// <summary>
        /// Registers a new user with the specified properties in the system.
        /// </summary>
        /// <param name="email">The new user's email address.</param>
        /// <param name="username">The new user's name.</param>
        /// <param name="password">The new user's password.</param>
        /// <param name="onSuccess">Delegate to be executed when the request is successful.</param>
        /// <param name="onError">Delegate to be executed when the request is not successful.</param>
        /// <returns>Async operation for the remote request.</returns>
        public static IEnumerator Register(string email, string username, string password, Action<string> onSuccess, Action<RegistrationError> onError)
        {
            return database.Register(email, username, password, onSuccess, onError);
        }

        /// <summary>
        /// Logs the specified user in the system.
        /// </summary>
        /// <param name="username">The user's name.</param>
        /// <param name="password">The user's password.</param>
        /// <param name="onSuccess">Delegate to be executed when the request is successful.</param>
        /// <param name="onError">Delegate to be executed when the request is not successful.</param>
        /// <returns>Async operation for the remote request.</returns>
        public static IEnumerator Login(string username, string password, Action<string> onSuccess, Action<LoginError> onError)
        {
            return database.Login(username, password, onSuccess, onError);
        }

        /// <summary>
        /// Gets the specified integer property from the specified user.
        /// </summary>
        /// <param name="username">The user's name.</param>
        /// <param name="key">The property's key.</param>
        /// <param name="onSuccess">Delegate to be executed when the request is successful.</param>
        /// <param name="onError">Delegate to be executed when the request is not successful.</param>
        /// <returns>Async operation for the remote request.</returns>
        public static IEnumerator GetIntProperty(string username, string key, Action<int> onSuccess, Action<string> onError)
        {
            return database.GetIntProperty(username, key, onSuccess, onError);
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
        public static IEnumerator SetIntProperty(string username, string key, int value, Action<int> onSuccess, Action<string> onError)
        {
            return database.SetIntProperty(username, key, value, onSuccess, onError);
        }

        /// <summary>
        /// Gets the specified string property from the specified user.
        /// </summary>
        /// <param name="username">The user's name.</param>
        /// <param name="key">The property's key.</param>
        /// <param name="onSuccess">Delegate to be executed when the request is successful.</param>
        /// <param name="onError">Delegate to be executed when the request is not successful.</param>
        /// <returns>Async operation for the remote request.</returns>
        public static IEnumerator GetStringProperty(string username, string key, Action<string> onSuccess, Action<string> onError)
        {
            return database.GetStringProperty(username, key, onSuccess, onError);
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
        public static IEnumerator SetStringProperty(string username, string key, string value, Action<string> onSuccess, Action<string> onError)
        {
            return database.SetStringProperty(username, key, value, onSuccess, onError);
        }
    }
}
