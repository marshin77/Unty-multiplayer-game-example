// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using MasterServerKit;

/// <summary>
/// This class manages the lobby screen used in the demo.
/// </summary>
public class LobbyScreen : MonoBehaviour
{
    /// <summary>
    /// Prefab for the match button.
    /// </summary>
    public GameObject matchButtonPrefab;

    /// <summary>
    /// Password input field.
    /// </summary>
    public InputField passwordInputField;

    /// <summary>
    /// UI text for the number of players.
    /// </summary>
    public Text numPlayersText;

    /// <summary>
    /// Cached reference to the lobby client.
    /// </summary>
    private BaseNetworkClient client;

    /// <summary>
    /// Cached reference to the chat dialog.
    /// </summary>
    private ChatDialog chatDialog;

    /// <summary>
    /// Unity's awake method.
    /// </summary>
    private void Awake()
    {
        Assert.IsTrue(matchButtonPrefab != null);
        Assert.IsTrue(passwordInputField != null);
        Assert.IsTrue(numPlayersText != null);
    }

    /// <summary>
    /// Unity's start method.
    /// </summary>
    private void Start()
    {
        client = ClientAPI.masterServerClient;
        StartCoroutine(Ping());
        OpenChatDialog();
    }

    /// <summary>
    /// This coroutine pings the master server every few seconds.
    /// This keeps the text showing the number of connected players
    /// up-to-date.
    /// </summary>
    /// <returns>Async operation for this method.</returns>
    private IEnumerator Ping()
    {
        while (true)
        {
            ClientAPI.Ping(numPlayers =>
            {
                numPlayersText.text = numPlayers.ToString();
            });
            yield return new WaitForSeconds(5f);
        }
    }

    /// <summary>
    /// Opens the lobby's chat dialog.
    /// </summary>
    private void OpenChatDialog()
    {
        WindowManager.Instance.OpenWindow("GameClient_ChatDialog", () =>
        {
            chatDialog = GameObject.Find("ChatDialog").GetComponent<ChatDialog>();
            var chatAddon = ClientAPI.GetMasterServerClientAddon<ChatClientAddon>();
            chatDialog.onSendButtonPressed = text =>
            {
                if (!string.IsNullOrEmpty(text))
                {
                    chatAddon.SendPublicChatMessage("Lobby", text);
                }
            };
            chatAddon.onReceivedPublicChatText = (channel, sender, text) =>
            {
                if (channel == "Lobby")
                {
                    if (chatDialog != null)
                    {
                        chatDialog.WriteText(sender + ": " + text);
                    }
                }
            };
        });
    }

    /// <summary>
    /// This method is called when the 'list matches' button is pressed.
    /// </summary>
    public void OnListMatchesButtonPressed()
    {
        var includeProperties = new List<Property>()
        {
        };
        var excludeProperties = new List<Property>()
        {
        };
        ClientAPI.FindGameRooms(includeProperties, excludeProperties, games =>
        {
            var matchesScrollView = GameObject.Find("MatchesScrollView/Viewport/Content");
            var matchesToRemove = new List<GameObject>();
            for (var i = 0; i < matchesScrollView.transform.childCount; i++)
            {
                matchesToRemove.Add(matchesScrollView.transform.GetChild(i).gameObject);
            }
            foreach (var match in matchesToRemove)
            {
                Destroy(match);
            }
            foreach (var game in games)
            {
                var matchButton = Instantiate(matchButtonPrefab);
                matchButton.GetComponent<MatchButton>().SetMatchData(game.id, game.isPrivate);
                matchButton.GetComponent<MatchButton>().labelTop.text = "Name: " + game.name;
                matchButton.GetComponent<MatchButton>().labelBottom.text = "Number of players: " + game.numPlayers + "/" + game.maxPlayers;
                matchButton.transform.SetParent(matchesScrollView.transform, false);
            }
        });
    }

    /// <summary>
    /// This method is called when the 'create match' button is pressed.
    /// </summary>
    public void OnCreateMatchButtonPressed()
    {
        var password = passwordInputField.text;
        WindowUtils.OpenLoadingDialog("Creating game...");
        ClientAPI.CreateGameRoom(client.username, 2, password, (ip, port) =>
        {
            JoinGameServer(ip, port);
        },
        error =>
        {
            WindowManager.Instance.CloseWindow("GameClient_LoadingDialog");
            WindowUtils.OpenAlertDialog("ERROR", "A game could not be created.");
        });
    }

    /// <summary>
    /// This method is called when the 'play now' button is pressed.
    /// </summary>
    public void OnPlayNowButtonPressed()
    {
        ClientAPI.PlayNow((ip, port) =>
        {
            WindowUtils.OpenLoadingDialog("Joining game...");
            JoinGameServer(ip, port);
        },
        (ip, port) =>
        {
            WindowUtils.OpenLoadingDialog("Creating game...");
            JoinGameServer(ip, port);
        },
        error =>
        {
            WindowManager.Instance.CloseWindow("GameClient_LoadingDialog");
            WindowUtils.OpenAlertDialog("ERROR", "No available games.");
        });
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
                SceneManager.LoadScene("GameClient_Game");
            });
        }
    }
}
