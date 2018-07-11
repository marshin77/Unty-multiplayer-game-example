// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

using MasterServerKit;

/// <summary>
/// This class manages the alert dialogs used in the demo.
/// </summary>
public class AlertDialog : MonoBehaviour
{
    /// <summary>
    /// UI text for the title.
    /// </summary>
    public Text titleText;

    /// <summary>
    /// UI text for the message.
    /// </summary>
    public Text messageText;

    /// <summary>
    /// Unity's awake method.
    /// </summary>
    private void Awake()
    {
        Assert.IsTrue(titleText != null);
        Assert.IsTrue(messageText != null);
    }

    /// <summary>
    /// This method is called when the accept button is pressed.
    /// </summary>
    public void OnAcceptButtonPressed()
    {
        WindowManager.Instance.CloseWindow("GameClient_AlertDialog");
    }

    /// <summary>
    /// Sets the title of this dialog.
    /// </summary>
    /// <param name="text">Title text.</param>
    public void SetTitle(string text)
    {
        titleText.text = text;
    }

    /// <summary>
    /// Sets the message text of this dialog.
    /// </summary>
    /// <param name="text">Message text.</param>
    public void SetMessage(string text)
    {
        messageText.text = text;
    }
}
