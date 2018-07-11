// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.SceneManagement;

using MasterServerKit;

/// <summary>
/// This class manages the login screen used in the demo.
/// </summary>
public class LoginScreen : MonoBehaviour
{
    /// <summary>
    /// This method is called when the login button is pressed.
    /// </summary>
    public void OnLoginButtonPressed()
    {
        SceneManager.LoadScene("GameClient_LoginDialog", LoadSceneMode.Additive);
    }

    /// <summary>
    /// This method is called when the register button is pressed.
    /// </summary>
    public void OnRegisterButtonPressed()
    {
        SceneManager.LoadScene("GameClient_RegisterDialog", LoadSceneMode.Additive);
    }

    /// <summary>
    /// This method is called when the guest button is pressed.
    /// </summary>
    public void OnGuestButtonPressed()
    {
        WindowUtils.OpenLoadingDialog("Entering as guest...");
        ClientAPI.LoginAsGuest(
            () =>
            {
                SceneManager.LoadScene("GameClient_Lobby");
            },
            error =>
            {
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
                WindowManager.Instance.CloseWindow("GameClient_LoadingDialog");
                WindowUtils.OpenAlertDialog("ERROR", errorMsg);
            });
    }
}
