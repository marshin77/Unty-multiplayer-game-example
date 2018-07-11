// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace MasterServerKit
{
    /// <summary>
    /// Manages the opening and closing of windows inside the game. Windows are scenes that are
    /// loaded additively into the current scene via the scene management facilities provided
    /// by Unity.
    /// </summary>
    public class WindowManager : MonoBehaviour
    {
        /// <summary>
        /// Singleton instance. The singleton is lazily instantiated for convenience (i.e., avoid
        /// the need to create an object in every scene of the game).
        /// </summary>
        private static WindowManager instance;

        /// <summary>
        /// The list of scene names that are pending to be unloaded.
        /// </summary>
        private List<string> pendingUnloads = new List<string>();

        public static WindowManager Instance
        {
            get
            {
                if (instance == null)
                {
                    var manager = new GameObject("WindowManager");
                    instance = manager.AddComponent<WindowManager>();
                    DontDestroyOnLoad(manager);
                }
                return instance;
            }
        }

        /// <summary>
        /// Unity's Update method.
        /// </summary>
        private void Update()
        {
            // We load the windows asynchronously, so we close them only when we
            // are sure they have actually been loaded in the first place.
            var completedUnloads = new List<string>();
            foreach (var sceneName in pendingUnloads)
            {
                var scene = SceneManager.GetSceneByName(sceneName);
                if (scene.IsValid())
                {
                    completedUnloads.Add(sceneName);
                    StartCoroutine(UnloadScene(sceneName));
                }
            }
            foreach (var sceneName in completedUnloads)
            {
                pendingUnloads.Remove(sceneName);
            }
        }

        /// <summary>
        /// Opens a new window with the specified name.
        /// </summary>
        /// <param name="name">Name of the scene containing the window to open.</param>
        /// <param name="onLoaded">Callback invoked when the window is loaded.</param>
        public void OpenWindow(string name, Action onLoaded)
        {
            StartCoroutine(LoadScene(name, onLoaded));
        }

        /// <summary>
        /// Closes the window with the specified name.
        /// </summary>
        /// <param name="name">Name of the window to close.</param>
        public void CloseWindow(string name)
        {
            pendingUnloads.Add(name);
        }

        /// <summary>
        /// Loads the scene with the specified name.
        /// </summary>
        /// <param name="name">Name of the scene to load.</param>
        /// <param name="onLoaded">Callback invoked when the scene is loaded.</param>
        /// <returns>Async operation for the scene loading.</returns>
        private IEnumerator LoadScene(string name, Action onLoaded)
        {
            var async = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
            yield return async;
            onLoaded();
        }

        /// <summary>
        /// Unloads the scene with the specified name.
        /// </summary>
        /// <param name="name">Name of the scene to unload.</param>
        /// <returns>Async operation for the scene unloading.</returns>
        private IEnumerator UnloadScene(string name)
        {
            var async = SceneManager.UnloadSceneAsync(name);
            yield return async;
        }
    }
}
