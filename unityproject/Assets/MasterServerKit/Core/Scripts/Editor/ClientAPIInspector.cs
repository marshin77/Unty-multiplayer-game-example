// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEditor;
using UnityEngine;

namespace MasterServerKit
{
    /// <summary>
    /// The custom inspector for the ClientAPI class.
    /// </summary>
    [CustomEditor(typeof(ClientAPI))]
    public class ClientAPIInspector : Editor
    {
        /// <summary>
        /// Unity's OnInspectorGUI method.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUIUtility.labelWidth = 160;

            EditorGUILayout.HelpBox("You specify the IP address and port of the master server here.", MessageType.Info);

            var ip = serializedObject.FindProperty("masterServerIp");
            EditorGUILayout.PropertyField(ip, new GUIContent("Master server IP address"), GUILayout.MaxWidth(300));

            var port = serializedObject.FindProperty("masterServerPort");
            EditorGUILayout.PropertyField(port, new GUIContent("Master server port"), GUILayout.MaxWidth(210));

            var useNetworkManager = serializedObject.FindProperty("useNetworkManagerPublic");
            EditorGUILayout.PropertyField(useNetworkManager, new GUIContent("Use Network Manager"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
