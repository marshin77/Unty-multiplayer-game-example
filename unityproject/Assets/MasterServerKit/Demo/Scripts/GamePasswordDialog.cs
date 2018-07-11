// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using MasterServerKit;

/// <summary>
/// This class manages the password dialog that is used when joining a private
/// match in the demo.
/// </summary>
public class GamePasswordDialog : MonoBehaviour
{
    /// <summary>
    /// Password input field.
    /// </summary>
    public InputField passwordInputField;

    /// <summary>
    /// The unique identifier of the match.
    /// </summary>
    private int matchId;

    /// <summary>
    /// Unity's awake method.
    /// </summary>
    private void Awake()
    {
        Assert.IsTrue(passwordInputField != null);
    }

    /// <summary>
    /// Sets the match information.
    /// </summary>
    /// <param name="matchId">The unique identifier of the match.</param>
    public void SetMatchData(int matchId)
    {
        this.matchId = matchId;
    }

    /// <summary>
    /// This method is called when the join button is pressed.
    /// </summary>
    public void OnJoinButtonPressed()
    {
        var passwordText = passwordInputField.text;

        // Perform some basic validation of the user input locally prior to calling the
        // remote join method. This is a good way to avoid some unnecessary network
        // traffic.
        if (string.IsNullOrEmpty(passwordText))
        {
            WindowUtils.OpenAlertDialog("ERROR", "Please enter a password.");
            return;
        }

        WindowUtils.OpenLoadingDialog("Joining game...");
        ClientAPI.JoinGameRoom(matchId, passwordText, (ip, port) =>
        {
            JoinGameServer(ip, port);
        },
        error =>
        {
            WindowManager.Instance.CloseWindow("GameClient_LoadingDialog");
            var errorMsg = "";
            switch (error)
            {
                case JoinGameRoomError.GameFull:
                    errorMsg = "This game is already full.";
                    break;

                case JoinGameRoomError.GameExpired:
                    errorMsg = "This game does not exist anymore.";
                    break;

                case JoinGameRoomError.InvalidPassword:
                    errorMsg = "Invalid password.";
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
        WindowManager.Instance.CloseWindow("GameClient_GamePasswordDialog");
    }

    /// <summary>
    /// Joins the game server at the specified IP address and port number.
    /// </summary>
    /// <param name="ip">IP address of the game server.</param>
    /// <param name="port">Port number of the game server.</param>
    private void JoinGameServer(string ip, int port)
    {
        if (ClientAPI.useNetworkManager)
        {
            ClientAPI.JoinGameServer(ip, port);
        }
        else
        {
            ClientAPI.JoinGameServer(ip, port, () =>
            {
                WindowManager.Instance.CloseWindow("GameClient_LoadingDialog");
                SceneManager.LoadScene("GameClient_Game");
            });
        }
    }
}
