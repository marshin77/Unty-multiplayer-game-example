// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using MasterServerKit;

/// <summary>
/// This class manages the login dialog used in the demo.
/// </summary>
public class LoginDialog : MonoBehaviour
{
    /// <summary>
    /// Username input field.
    /// </summary>
    public InputField usernameInputField;

    /// <summary>
    /// Password input field.
    /// </summary>
    public InputField passwordInputField;

    /// <summary>
    /// Unity's awake method.
    /// </summary>
    private void Awake()
    {
        Assert.IsTrue(usernameInputField != null);
        Assert.IsTrue(passwordInputField != null);
    }

    /// <summary>
    /// This method is called when the login button is pressed.
    /// </summary>
    public void OnLoginButtonPressed()
    {
        var usernameText = usernameInputField.text;
        var passwordText = passwordInputField.text;

        // Perform some basic validation of the user input locally prior to calling the
        // remote login method. This is a good way to avoid some unnecessary network
        // traffic.
        if (string.IsNullOrEmpty(usernameText))
        {
            WindowUtils.OpenAlertDialog("ERROR", "Please enter your username.");
            return;
        }

        if (string.IsNullOrEmpty(passwordText))
        {
            WindowUtils.OpenAlertDialog("ERROR", "Please enter your password.");
            return;
        }

        WindowUtils.OpenLoadingDialog("Authenticating user...");
        ClientAPI.Login(usernameText, passwordText,
            () =>
            {
                SceneManager.LoadScene("GameClient_Lobby");
            },
            error =>
            {
                WindowManager.Instance.CloseWindow("GameClient_LoadingDialog");
                var errorMsg = "";
                switch (error)
                {
                    case LoginError.DatabaseConnectionError:
                        errorMsg = "There was an error connecting to the database.";
                        break;

                    case LoginError.NonexistingUser:
                        errorMsg = "This user does not exist.";
                        break;

                    case LoginError.InvalidCredentials:
                        errorMsg = "Invalid credentials.";
                        break;

                    case LoginError.ServerFull:
                        errorMsg = "The server is full.";
                        break;

                    case LoginError.AuthenticationRequired:
                        errorMsg = "Authentication is required.";
                        break;

                    case LoginError.UserAlreadyLoggedIn:
                        errorMsg = "This user is already logged in.";
                        break;
                }
                WindowUtils.OpenAlertDialog("ERROR", errorMsg);
            });
    }

    /// <summary>
    /// This method is called when the close button is pressed.
    /// </summary>
    public void OnCloseButtonPressed()
    {
        WindowManager.Instance.CloseWindow("GameClient_LoginDialog");
    }
}
