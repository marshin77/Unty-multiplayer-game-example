// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

/// <summary>
/// This class manages the chat dialogs used in the demo.
/// </summary>
public class ChatDialog : MonoBehaviour
{
    /// <summary>
    /// Text prefab for the chat lines.
    /// </summary>
    public GameObject textPrefab;

    /// <summary>
    /// UI scroll rect for the chat.
    /// </summary>
    public ScrollRect scrollRect;

    /// <summary>
    /// UI scroll view content for the chat.
    /// </summary>
    public GameObject scrollViewContent;

    /// <summary>
    /// Chat input field.
    /// </summary>
    public InputField textInputField;

    /// <summary>
    /// Callback executed when the send button is pressed.
    /// </summary>
    public Action<string> onSendButtonPressed;

    /// <summary>
    /// True if this dialog is open; false otherwise.
    /// </summary>
    private bool open;

    /// <summary>
    /// Unity's awake method.
    /// </summary>
    private void Awake()
    {
        Assert.IsTrue(textPrefab != null);
        Assert.IsTrue(scrollRect != null);
        Assert.IsTrue(scrollViewContent != null);
        Assert.IsTrue(textInputField != null);
    }

    /// <summary>
    /// Unity's start method.
    /// </summary>
    private void Start()
    {
        textInputField.ActivateInputField();
        Close();
    }

    /// <summary>
    /// Opens this dialog.
    /// </summary>
    private void Open()
    {
        var canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1.0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        open = true;
    }

    /// <summary>
    /// Closes this dialog.
    /// </summary>
    private void Close()
    {
        var canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0.0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        open = false;
    }

    /// <summary>
    /// Switches this dialog between opened/closed as appropriate.
    /// </summary>
    public void Switch()
    {
        if (open)
        {
            Close();
        }
        else
        {
            Open();
        }
    }

    /// <summary>
    /// This method is called when the close button is pressed.
    /// </summary>
    public void OnCloseButtonPressed()
    {
        Close();
    }

    /// <summary>
    /// This method is called when the send button is pressed.
    /// </summary>
    public void OnSendButtonPressed()
    {
        if (onSendButtonPressed != null)
        {
            onSendButtonPressed(textInputField.text);
        }
        textInputField.text = "";
        textInputField.ActivateInputField();
    }

    /// <summary>
    /// This method is called when the user has finished editing the dialog's input
    /// field.
    /// </summary>
    public void OnChatInputFieldEditEnded()
    {
        // It seems Unity's InputField OnEndEdit event is called in a lot of contexts
        // other than submitting the text from an input field (e.g, clicking on a
        // scrollbar), so make sure we got here only by pressing Enter on an input
        // field.
        if (!Input.GetButtonDown("Submit"))
            return;

        OnSendButtonPressed();
    }

    /// <summary>
    /// Writes the specified text to the chat.
    /// </summary>
    /// <param name="text">Text to write to the chat.</param>
    public void WriteText(string text)
    {
        var textObj = Instantiate(textPrefab);
        textObj.GetComponent<Text>().text = text;
        textObj.transform.SetParent(scrollViewContent.transform, false);
        scrollRect.velocity = new Vector2(0.0f, 1000.0f);
    }
}
