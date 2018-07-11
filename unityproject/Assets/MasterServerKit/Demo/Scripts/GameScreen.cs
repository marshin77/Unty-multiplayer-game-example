// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

using MasterServerKit;

/// <summary>
/// This class manages the game screen used in the demo.
/// </summary>
public class GameScreen : MonoBehaviour
{
    /// <summary>
    /// Cached reference to the current game client.
    /// </summary>
    private GameClient gameClient;

    /// <summary>
    /// Cached reference to the chat dialog.
    /// </summary>
    private ChatDialog chatDialog;

    /// <summary>
    /// Unity's start method.
    /// </summary>
    private void Start()
    {
        if (ClientAPI.gameClient != null)
        {
            gameClient = ClientAPI.gameClient;
            OpenChatDialog();
        }
    }

    /// <summary>
    /// Opens the game's chat dialog.
    /// </summary>
    private void OpenChatDialog()
    {
        WindowManager.Instance.OpenWindow("GameClient_ChatDialog", () =>
        {
            chatDialog = GameObject.Find("ChatDialog").GetComponent<ChatDialog>();
            var chatAddon = ClientAPI.GetGameClientAddon<ChatClientAddon>();
            chatDialog.onSendButtonPressed = text =>
            {
                if (!string.IsNullOrEmpty(text))
                {
                    chatAddon.SendPublicChatMessage("Game", text);
                }
            };
            chatAddon.onReceivedPublicChatText = (channel, sender, text) =>
            {
                if (channel == "Game")
                {
                    chatDialog.WriteText(sender + ": " + text);
                }
            };
        });
    }

    /// <summary>
    /// This method is called when the 'exit to lobby' button is pressed.
    /// </summary>
    public void OnExitToLobbyButtonPressed()
    {
        if (ClientAPI.useNetworkManager)
        {
            NetworkManager.singleton.StopClient();
        }
        else
        {
            gameClient.client.Disconnect();
            SceneManager.LoadScene("GameClient_Lobby");
        }
    }

    /// <summary>
    /// This method is called when the chat button is pressed.
    /// </summary>
    public void OnChatButtonPressed()
    {
        if (chatDialog != null)
        {
            chatDialog.Switch();
        }
    }
}
