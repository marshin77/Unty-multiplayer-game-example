// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using LiteDB;

namespace MasterServerKit
{
    // POCO class entity for users.
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public Dictionary<string, int> IntProperties { get; set; }
        public Dictionary<string, string> StringProperties { get; set; }
    }

    /// <summary>
    /// LiteDB database provider implementation.
    /// </summary>
    public class LiteDBProvider : IDatabaseProvider
    {
        /// <summary>
        /// Name of the LiteDB database.
        /// </summary>
        private readonly string databaseName = "database";

        /// <summary>
        /// The LiteDB database.
        /// </summary>
        private LiteDatabase database;

        /// <summary>
        /// Performs any initialization-related logic.
        /// </summary>
        public void InitializeDatabase()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            database = new LiteDatabase(path + "/" + databaseName + ".db");
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

            var users = database.GetCollection<User>("User");

            var usersWithTheSameEmail = users.Find(x => x.Email == email);
            if (usersWithTheSameEmail.Count() > 0)
            {
                onError(RegistrationError.AlreadyExistingEmailAddress);
                yield break;
            }

            var usersWithTheSameName = users.Find(x => x.Name == username);
            if (usersWithTheSameName.Count() > 0)
            {
                onError(RegistrationError.AlreadyExistingUsername);
                yield break;
            }

            var passwordToStore = CryptoUtils.GenerateCombinedSaltAndHash(password);

            var newUser = new User
            {
                Name = username,
                Email = email,
                Password = passwordToStore,
                IntProperties = new Dictionary<string, int>(),
                StringProperties = new Dictionary<string, string>()
            };

            users.Insert(newUser);
            users.EnsureIndex(x => x.Name);

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

            var users = database.GetCollection<User>("User");

            var usersWithSameName = users.Find(x => x.Name == username);
            if (usersWithSameName.Count() == 0)
            {
                onError(LoginError.NonexistingUser);
                yield break;
            }

            var user = usersWithSameName.ElementAt(0);
            var storedPassword = user.Password;
            if (!CryptoUtils.CheckPasswordsAreEqual(password, storedPassword))
            {
                onError(LoginError.InvalidCredentials);
                yield break;
            }

            users.Update(user);

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
            var users = database.GetCollection<User>("User");

            var usersWithSameName = users.Find(x => x.Name == username);
            if (usersWithSameName.Count() == 0)
            {
                onError("This user does not exist.");
                yield break;
            }

            var user = usersWithSameName.ElementAt(0);
            if (user.IntProperties.ContainsKey(key))
            {
                onSuccess(user.IntProperties[key]);
                yield break;
            }
            else
            {
                onError("This property does not exist");
            }
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
            var users = database.GetCollection<User>("User");

            var usersWithSameName = users.Find(x => x.Name == username);
            if (usersWithSameName.Count() == 0)
            {
                onError("This user does not exist.");
                yield break;
            }

            var user = usersWithSameName.ElementAt(0);
            user.IntProperties[key] = value;
            users.Update(user);
            onSuccess(user.IntProperties[key]);
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
            var users = database.GetCollection<User>("User");

            var usersWithSameName = users.Find(x => x.Name == username);
            if (usersWithSameName.Count() == 0)
            {
                onError("This user does not exist.");
                yield break;
            }

            var user = usersWithSameName.ElementAt(0);
            if (user.StringProperties.ContainsKey(key))
            {
                onSuccess(user.StringProperties[key]);
                yield break;
            }
            else
            {
                onError("This property does not exist");
            }
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
            var users = database.GetCollection<User>("User");

            var usersWithSameName = users.Find(x => x.Name == username);
            if (usersWithSameName.Count() == 0)
            {
                onError("This user does not exist.");
                yield break;
            }

            var user = usersWithSameName.ElementAt(0);
            user.StringProperties[key] = value;
            users.Update(user);
            onSuccess(user.StringProperties[key]);
        }
    }
}
