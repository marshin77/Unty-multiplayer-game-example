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
/// This class manages the match button used in the demo.
/// </summary>
public class MatchButton : MonoBehaviour
{
    /// <summary>
    /// UI text for the top label.
    /// </summary>
    public Text labelTop;

    /// <summary>
    /// UI text for the bottom label.
    /// </summary>
    public Text labelBottom;

    /// <summary>
    /// UI image for the lock icon.
    /// </summary>
    public Image lockImage;

    /// <summary>
    /// The unique identifier of the match.
    /// </summary>
    private int matchId;

    /// <summary>
    /// True if the match is private; false otherwise.
    /// </summary>
    private bool isPrivate;

    /// <summary>
    /// Unity's awake method.
    /// </summary>
    private void Awake()
    {
        Assert.IsTrue(labelTop != null);
        Assert.IsTrue(labelBottom != null);
        Assert.IsTrue(lockImage != null);
    }

    /// <summary>
    /// Sets the match information.
    /// </summary>
    /// <param name="id">The unique identifier of the match.</param>
    /// <param name="isPrivate">True if the match is private; false otherwise.</param>
    public void SetMatchData(int id, bool isPrivate)
    {
        matchId = id;
        this.isPrivate = isPrivate;
        lockImage.gameObject.SetActive(isPrivate);
    }

    /// <summary>
    /// This method is called when the button is pressed.
    /// </summary>
    public void OnButtonPressed()
    {
        if (isPrivate)
        {
            WindowManager.Instance.OpenWindow("GameClient_GamePasswordDialog", () =>
            {
                var dialog = GameObject.Find("GamePasswordDialog").GetComponent<GamePasswordDialog>();
                dialog.SetMatchData(matchId);
            });
        }
        else
        {
            WindowUtils.OpenLoadingDialog("Joining game...");
            ClientAPI.JoinGameRoom(matchId, null, (ip, port) =>
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
