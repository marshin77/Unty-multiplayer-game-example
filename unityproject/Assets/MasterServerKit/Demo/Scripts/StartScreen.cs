// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.SceneManagement;

using MasterServerKit;

/// <summary>
/// This class manages the start screen used in the demo.
/// </summary>
public class StartScreen : MonoBehaviour
{
    /// <summary>
    /// This method is called when the 'Game Rooms demo' button is pressed.
    /// </summary>
    public void OnGameRoomsDemoButtonPressed()
    {
        WindowUtils.OpenLoadingDialog("Connecting to master server...");
        ClientAPI.ConnectToMasterServer(() =>
        {
            SceneManager.LoadScene("GameClient_Login");
        },
        () =>
        {
            WindowManager.Instance.CloseWindow("GameClient_LoadingDialog");
            WindowUtils.OpenAlertDialog("ERROR", "Could not connect to master server.");
        });
    }
}
