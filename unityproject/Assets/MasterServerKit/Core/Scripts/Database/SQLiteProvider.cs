// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections;

using Mono.Data.Sqlite;

namespace MasterServerKit
{
    /// <summary>
    /// SQLite database provider implementation.
    /// </summary>
    public class SQLiteProvider : IDatabaseProvider
    {
        /// <summary>
        /// Name of the SQLite database.
        /// </summary>
        private readonly string databaseName = "database";

        /// <summary>
        /// Connection to the SQLite database.
        /// </summary>
        private SqliteConnection connection;

        /// <summary>
        /// Performs any initialization-related logic.
        /// </summary>
        public void InitializeDatabase()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var dbPath = "URI=file:" + path + "/" + databaseName + ".s3db";
            connection = new SqliteConnection(dbPath);
            connection.Open();

            var command = connection.CreateCommand();
            var sql = @"CREATE TABLE IF NOT EXISTS User (
                id INTEGER PRIMARY KEY,
                username TEXT NOT NULL UNIQUE,
                email TEXT NOT NULL UNIQUE,
                password TEXT NOT NULL
            )";
            command.CommandText = sql;
            command.ExecuteNonQuery();

            sql = @"CREATE TABLE IF NOT EXISTS IntProperty (
                id INTEGER PRIMARY KEY,
                user_id INTEGER NOT NULL,
                key TEXT NOT NULL UNIQUE,
                value INTEGER NOT NULL,
                FOREIGN KEY(user_id) REFERENCES User(id)
            )";
            command.CommandText = sql;
            command.ExecuteNonQuery();

            sql = @"CREATE TABLE IF NOT EXISTS StringProperty (
                id INTEGER PRIMARY KEY,
                user_id INTEGER NOT NULL,
                key TEXT NOT NULL UNIQUE,
                value TEXT NOT NULL,
                FOREIGN KEY(user_id) REFERENCES User(id)
            )";
            command.CommandText = sql;
            command.ExecuteNonQuery();
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

            var command = connection.CreateCommand();
            var sql = "SELECT * from User WHERE email = @email";
            command.CommandText = sql;
            command.Parameters.AddWithValue("@email", email);
            var reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                onError(RegistrationError.AlreadyExistingEmailAddress);
                reader.Close();
                yield break;
            }
            reader.Close();

            sql = "SELECT * from User WHERE username = @username";
            command.CommandText = sql;
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@username", username);
            reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                onError(RegistrationError.AlreadyExistingUsername);
                reader.Close();
                yield break;
            }
            reader.Close();

            var passwordToStore = CryptoUtils.GenerateCombinedSaltAndHash(password);

            sql = "INSERT INTO User (username, email, password) VALUES (@username, @email, @password)";
            command.CommandText = sql;
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@email", email);
            command.Parameters.AddWithValue("@password", passwordToStore);
            command.ExecuteNonQuery();

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

            var command = connection.CreateCommand();
            var sql = "SELECT * from User WHERE username = @username";
            command.CommandText = sql;
            command.Parameters.AddWithValue("@username", username);
            var reader = command.ExecuteReader();
            if (!reader.HasRows)
            {
                onError(LoginError.NonexistingUser);
                reader.Close();
                yield break;
            }

            reader.Read();
            var storedPassword = reader["password"] as string;
            if (!CryptoUtils.CheckPasswordsAreEqual(password, storedPassword))
            {
                onError(LoginError.InvalidCredentials);
                reader.Close();
                yield break;
            }
            reader.Close();

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
            var command = connection.CreateCommand();
            var sql = "SELECT * from User WHERE username = @username";
            command.CommandText = sql;
            command.Parameters.AddWithValue("@username", username);
            var reader = command.ExecuteReader();
            if (!reader.HasRows)
            {
                onError("This user does not exist.");
                reader.Close();
                yield break;
            }

            reader.Read();
            var userId = reader.GetInt32(reader.GetOrdinal("id"));
            reader.Close();

            sql = "SELECT * from IntProperty WHERE user_id = @user_id AND key = @key";
            command.CommandText = sql;
            command.Parameters.AddWithValue("@user_id", userId);
            command.Parameters.AddWithValue("@key", key);
            reader = command.ExecuteReader();
            if (!reader.HasRows)
            {
                onError("This property does not exist.");
                reader.Close();
                yield break;
            }

            onSuccess(Convert.ToInt32(reader["value"]));
            reader.Close();
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
            var command = connection.CreateCommand();
            var sql = "SELECT * from User WHERE username = @username";
            command.CommandText = sql;
            command.Parameters.AddWithValue("@username", username);
            var reader = command.ExecuteReader();
            if (!reader.HasRows)
            {
                onError("This user does not exist.");
                reader.Close();
                yield break;
            }

            reader.Read();
            var userId = reader.GetInt32(reader.GetOrdinal("id"));
            reader.Close();

            sql = "SELECT * from IntProperty WHERE user_id = @user_id AND key = @key";
            command.CommandText = sql;
            command.Parameters.AddWithValue("@user_id", userId);
            command.Parameters.AddWithValue("@key", key);
            reader = command.ExecuteReader();
            if (!reader.HasRows)
            {
                reader.Close();

                sql = "INSERT INTO IntProperty (user_id, key, value) VALUES (@user_id, @key, @value)";
                command.CommandText = sql;
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@user_id", userId);
                command.Parameters.AddWithValue("@key", key);
                command.Parameters.AddWithValue("@value", value);
                command.ExecuteNonQuery();
            }

            reader.Close();
            sql = "UPDATE IntProperty SET value = @value WHERE user_id = @user_id AND key = @key";
            command.CommandText = sql;
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@value", value);
            command.Parameters.AddWithValue("@user_id", userId);
            command.Parameters.AddWithValue("@key", key);
            command.ExecuteNonQuery();

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
            var command = connection.CreateCommand();
            var sql = "SELECT * from User WHERE username = @username";
            command.CommandText = sql;
            command.Parameters.AddWithValue("@username", username);
            var reader = command.ExecuteReader();
            if (!reader.HasRows)
            {
                onError("This user does not exist.");
                reader.Close();
                yield break;
            }

            reader.Read();
            var userId = reader.GetInt32(reader.GetOrdinal("id"));
            reader.Close();

            sql = "SELECT * from StringProperty WHERE user_id = @user_id AND key = @key";
            command.CommandText = sql;
            command.Parameters.AddWithValue("@user_id", userId);
            command.Parameters.AddWithValue("@key", key);
            reader = command.ExecuteReader();
            if (!reader.HasRows)
            {
                onError("This property does not exist.");
                reader.Close();
                yield break;
            }

            onSuccess(reader["value"] as string);
            reader.Close();
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
            var command = connection.CreateCommand();
            var sql = "SELECT * from User WHERE username = @username";
            command.CommandText = sql;
            command.Parameters.AddWithValue("@username", username);
            var reader = command.ExecuteReader();
            if (!reader.HasRows)
            {
                onError("This user does not exist.");
                reader.Close();
                yield break;
            }

            reader.Read();
            var userId = reader.GetInt32(reader.GetOrdinal("id"));
            reader.Close();

            sql = "SELECT * from StringProperty WHERE user_id = @user_id AND key = @key";
            command.CommandText = sql;
            command.Parameters.AddWithValue("@user_id", userId);
            command.Parameters.AddWithValue("@key", key);
            reader = command.ExecuteReader();
            if (!reader.HasRows)
            {
                reader.Close();

                sql = "INSERT INTO StringProperty (user_id, key, value) VALUES (@user_id, @key, @value)";
                command.CommandText = sql;
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@user_id", userId);
                command.Parameters.AddWithValue("@key", key);
                command.Parameters.AddWithValue("@value", value);
                command.ExecuteNonQuery();
            }

            reader.Close();
            sql = "UPDATE StringProperty SET value = @value WHERE user_id = @user_id AND key = @key";
            command.CommandText = sql;
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@value", value);
            command.Parameters.AddWithValue("@user_id", userId);
            command.Parameters.AddWithValue("@key", key);
            command.ExecuteNonQuery();

            onSuccess(value);
        }
    }
}