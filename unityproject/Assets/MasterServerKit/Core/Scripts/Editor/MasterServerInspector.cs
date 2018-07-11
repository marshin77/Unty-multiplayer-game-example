// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEditor;
using UnityEngine;

namespace MasterServerKit
{
    /// <summary>
    /// The custom inspector for the MasterServer class.
    /// </summary>
    [CustomEditor(typeof(MasterServer), true)]
    public class MasterServerInspector : Editor
    {
        /// <summary>
        /// Unity's OnInspectorGUI method.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUIUtility.labelWidth = 150;

            var helpText = "The master server is the entry point to Master Server Kit. You specify the settings of your master server here.";
            EditorGUILayout.HelpBox(helpText, MessageType.Info);

            var ip = serializedObject.FindProperty("ip");
            EditorGUILayout.PropertyField(ip, new GUIContent("IP address"), GUILayout.MaxWidth(300));

            var port = serializedObject.FindProperty("port");
            EditorGUILayout.PropertyField(port, new GUIContent("Port"), GUILayout.MaxWidth(210));

            var playerLimit = serializedObject.FindProperty("playerLimit");
            EditorGUILayout.PropertyField(playerLimit, new GUIContent("Player limit"));

            if (playerLimit.boolValue)
            {
                var maxPlayers = serializedObject.FindProperty("maxPlayers");
                EditorGUILayout.PropertyField(maxPlayers, new GUIContent("Max players"), GUILayout.MaxWidth(210));
            }

            var allowGuests = serializedObject.FindProperty("allowGuests");
            EditorGUILayout.PropertyField(allowGuests, new GUIContent("Allow guests"));

            if (allowGuests.boolValue)
            {
                EditorGUI.indentLevel++;

                var guestName = serializedObject.FindProperty("guestName");
                EditorGUILayout.PropertyField(guestName, new GUIContent("Guest name"), GUILayout.MaxWidth(300));

                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
