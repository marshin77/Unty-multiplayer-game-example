// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

/// <summary>
/// This class manages the loading dialogs used in the demo.
/// </summary>
public class LoadingDialog : MonoBehaviour
{
    /// <summary>
    /// UI text for the message.
    /// </summary>
    public Text messageText;

    /// <summary>
    /// Unity's awake method.
    /// </summary>
    private void Awake()
    {
        Assert.IsTrue(messageText != null);
    }

    /// <summary>
    /// Sets the message of this dialog.
    /// </summary>
    /// <param name="text">Message text.</param>
    public void SetMessage(string text)
    {
        messageText.text = text;
    }
}
