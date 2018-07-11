// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Text.RegularExpressions;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

using MasterServerKit;

/// <summary>
/// This class manages the register dialog used in the demo.
/// </summary>
public class RegisterDialog : MonoBehaviour
{
    /// <summary>
    /// Email input field.
    /// </summary>
    public InputField emailInputField;

    /// <summary>
    /// Username input field.
    /// </summary>
    public InputField usernameInputField;

    /// <summary>
    /// Password input field.
    /// </summary>
    public InputField passwordInputField;

    /// <summary>
    /// Password check input field.
    /// </summary>
    public InputField passwordCheckInputField;

    /// <summary>
    /// Unity's awake method.
    /// </summary>
    private void Awake()
    {
        Assert.IsTrue(emailInputField != null);
        Assert.IsTrue(usernameInputField != null);
        Assert.IsTrue(passwordInputField != null);
        Assert.IsTrue(passwordCheckInputField != null);
    }

    /// <summary>
    /// This method is called when the register button is pressed.
    /// </summary>
    public void OnRegisterButtonPressed()
    {
        var emailText = emailInputField.text;
        var usernameText = usernameInputField.text;
        var passwordText = passwordInputField.text;
        var passwordCheckText = passwordCheckInputField.text;

        // Perform some basic validation of the user input locally prior to calling the
        // remote registration method. This is a good way to avoid some unnecessary
        // network traffic.
        if (string.IsNullOrEmpty(emailText))
        {
            WindowUtils.OpenAlertDialog("ERROR", "Please enter your email address.");
            return;
        }

        if (!IsValidEmail(emailText))
        {
            WindowUtils.OpenAlertDialog("ERROR", "The email address you entered is not valid.");
            return;
        }

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

        if (passwordText != passwordCheckText)
        {
            WindowUtils.OpenAlertDialog("ERROR", "The passwords do not match.");
            return;
        }

        ClientAPI.Register(emailText, usernameText, passwordText,
            () =>
            {
                WindowManager.Instance.CloseWindow("GameClient_RegisterDialog");
            },
            error =>
            {
                var errorMsg = "";
                switch (error)
                {
                    case RegistrationError.DatabaseConnectionError:
                        errorMsg = "There was an error connecting to the database.";
                        break;

                    case RegistrationError.MissingEmailAddress:
                        errorMsg = "You need to enter an email address.";
                        break;

                    case RegistrationError.MissingUsername:
                        errorMsg = "You need to enter a username.";
                        break;

                    case RegistrationError.MissingPassword:
                        errorMsg = "You need to enter a password.";
                        break;

                    case RegistrationError.AlreadyExistingEmailAddress:
                        errorMsg = "This email adress is already registered.";
                        break;

                    case RegistrationError.AlreadyExistingUsername:
                        errorMsg = "This username is already registered.";
                        break;
                }
                WindowUtils.OpenAlertDialog("ERROR", errorMsg);
            });
    }

    /// <summary>
    /// Checks if the specified email address is valid.
    /// </summary>
    /// <param name="email">Email address to check.</param>
    /// <returns>True if the specified email address is valid; false otherwise.</returns>
    private bool IsValidEmail(string email)
    {
        return Regex.IsMatch(email,
            @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z",
            RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// This method is called when the close button is pressed.
    /// </summary>
    public void OnCloseButtonPressed()
    {
        WindowManager.Instance.CloseWindow("GameClient_RegisterDialog");
    }
}
