// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections;

using UnityEngine;
using UnityEngine.Networking;

namespace MasterServerKit
{
    /// <summary>
    /// Helper struct to store the response of the registration remote request.
    /// </summary>
    [Serializable]
    public struct RegisterResponse
    {
        public string status;
        public string message;
        public string token;
        public int error;
    }

    /// <summary>
    /// Helper struct to store the response of the login remote request.
    /// </summary>
    [Serializable]
    public struct LoginResponse
    {
        public string status;
        public string message;
        public string token;
        public string username;
        public int error;
    }

    /// <summary>
    /// Helper struct to store the response of the get property remote request.
    /// </summary>
    [Serializable]
    public struct GetPropertyResponse
    {
        public string status;
        public string message;
        public string value;
    }

    /// <summary>
    /// Helper struct to store the response of the set property remote request.
    /// </summary>
    [Serializable]
    public struct SetPropertyResponse
    {
        public string status;
        public string message;
        public string value;
    }

    /// <summary>
    /// Helper struct to store the response of the reset logged in status remote request.
    /// </summary>
    [Serializable]
    public struct ResetLoggedInStatusResponse
    {
        public string status;
        public string message;
    }

    /// <summary>
    /// MongoDB database provider implementation.
    /// </summary>
    public class MongoDBProvider : IDatabaseProvider
    {
        /// <summary>
        /// The URL of the Node.js server that provides a REST-based API to access
        /// the MongoDB database.
        /// </summary>
        private static readonly string url = "http://127.0.0.1:8080/api/";

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
            var form = new WWWForm();
            form.AddField("email", email);
            form.AddField("username", username);
            form.AddField("password", password);

            var www = UnityWebRequest.Post(url + "register", form);
            yield return www.Send();

            if (www.isNetworkError)
            {
                onError(RegistrationError.DatabaseConnectionError);
            }
            else
            {
                try
                {
                    var response = JsonUtility.FromJson<RegisterResponse>(www.downloadHandler.text);
                    if (response.status == "success")
                    {
                        onSuccess(response.token);
                    }
                    else if (response.status == "error")
                    {
                        onError((RegistrationError)response.error);
                    }
                }
                catch (Exception)
                {
                    onError(RegistrationError.DatabaseConnectionError);
                }
            }
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
            var form = new WWWForm();
            form.AddField("username", username);
            form.AddField("password", password);

            var www = UnityWebRequest.Post(url + "login", form);
            yield return www.Send();

            if (www.isNetworkError)
            {
                onError(LoginError.DatabaseConnectionError);
            }
            else
            {
                try
                {
                    var response = JsonUtility.FromJson<LoginResponse>(www.downloadHandler.text);
                    if (response.status == "success")
                    {
                        onSuccess(response.username);
                    }
                    else if (response.status == "error")
                    {
                        onError((LoginError)response.error);
                    }
                }
                catch (Exception)
                {
                    onError(LoginError.DatabaseConnectionError);
                }
            }
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
            var www = UnityWebRequest.Get(url + "getIntProperty?username=" + username + "&key=" + key);
            yield return www.Send();

            if (www.isNetworkError)
            {
                onError(www.error);
            }
            else
            {
                try
                {
                    var response = JsonUtility.FromJson<GetPropertyResponse>(www.downloadHandler.text);
                    if (response.status == "success")
                    {
                        onSuccess(int.Parse(response.value));
                    }
                    else if (response.status == "error")
                    {
                        onError(response.message);
                    }
                }
                catch (Exception)
                {
                    onError("Could not connect to server.");
                }
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
            var form = new WWWForm();
            form.AddField("username", username);
            form.AddField("key", key);
            form.AddField("value", value);

            var www = UnityWebRequest.Post(url + "setIntProperty", form);
            yield return www.Send();

            if (www.isNetworkError)
            {
                onError(www.error);
            }
            else
            {
                try
                {
                    var response = JsonUtility.FromJson<SetPropertyResponse>(www.downloadHandler.text);
                    if (response.status == "success")
                    {
                        onSuccess(int.Parse(response.value));
                    }
                    else if (response.status == "error")
                    {
                        onError(response.message);
                    }
                }
                catch (Exception)
                {
                    onError("Could not connect to server.");
                }
            }
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
            var www = UnityWebRequest.Get(url + "getStringProperty?username=" + username + "&key=" + key);
            yield return www.Send();

            if (www.isNetworkError)
            {
                onError(www.error);
            }
            else
            {
                try
                {
                    var response = JsonUtility.FromJson<GetPropertyResponse>(www.downloadHandler.text);
                    if (response.status == "success")
                    {
                        onSuccess(response.value);
                    }
                    else if (response.status == "error")
                    {
                        onError(response.message);
                    }
                }
                catch (Exception)
                {
                    onError("Could not connect to server.");
                }
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
            var form = new WWWForm();
            form.AddField("username", username);
            form.AddField("key", key);
            form.AddField("value", value);

            var www = UnityWebRequest.Post(url + "setStringProperty", form);
            yield return www.Send();

            if (www.isNetworkError)
            {
                onError(www.error);
            }
            else
            {
                try
                {
                    var response = JsonUtility.FromJson<SetPropertyResponse>(www.downloadHandler.text);
                    if (response.status == "success")
                    {
                        onSuccess(response.value);
                    }
                    else if (response.status == "error")
                    {
                        onError(response.message);
                    }
                }
                catch (Exception)
                {
                    onError("Could not connect to server.");
                }
            }
        }
    }
}
