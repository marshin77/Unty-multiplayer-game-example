// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

namespace MasterServerKit
{
    /// <summary>
    /// Helper functions for opening the generic dialogs in the accompanying demo.
    /// </summary>
    public static class WindowUtils
    {
        /// <summary>
        /// Opens a new alert dialog.
        /// </summary>
        /// <param name="title">The alert dialog's title.</param>
        /// <param name="text">The alert dialog's text.</param>
        public static void OpenAlertDialog(string title, string text)
        {
            WindowManager.Instance.OpenWindow("GameClient_AlertDialog", () =>
            {
                var dialog = GameObject.Find("AlertDialog").GetComponent<AlertDialog>();
                dialog.SetTitle(title);
                dialog.SetMessage(text);
            });
        }

        /// <summary>
        /// Opens a new loading dialog.
        /// </summary>
        /// <param name="text">The loading dialog's text.</param>
        public static void OpenLoadingDialog(string text)
        {
            WindowManager.Instance.OpenWindow("GameClient_LoadingDialog", () =>
            {
                var dialog = GameObject.Find("LoadingDialog").GetComponent<LoadingDialog>();
                dialog.SetMessage(text);
            });
        }
    }
}
