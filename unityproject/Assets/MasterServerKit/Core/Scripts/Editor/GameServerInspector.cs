// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MasterServerKit
{
    /// <summary>
    /// The custom inspector for the GameServer class.
    /// </summary>
    [CustomEditor(typeof(GameServer), true)]
    public class GameServerInspector : Editor
    {
        /// <summary>
        /// The list of properties of this game server.
        /// </summary>
        private ReorderableList propertiesList;

        /// <summary>
        /// Unity's OnEnable method.
        /// </summary>
        private void OnEnable()
        {
            propertiesList = new ReorderableList(serializedObject, serializedObject.FindProperty("properties"), true, true, true, true);
            propertiesList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Properties");
            };
            propertiesList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = propertiesList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                EditorGUI.LabelField(new Rect(rect.x, rect.y, 40, EditorGUIUtility.singleLineHeight), "Name");
                EditorGUI.PropertyField(new Rect(rect.x + 40, rect.y, 120, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("name"), GUIContent.none);
                EditorGUI.LabelField(new Rect(rect.x + 40 + 120 + 10, rect.y, 40, EditorGUIUtility.singleLineHeight), "Value");
                EditorGUI.PropertyField(new Rect(rect.x + 40 + 120 + 10 + 40, rect.y, 120, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("value"), GUIContent.none);
            };
            propertiesList.onAddCallback = (ReorderableList l) =>
            {
                var index = l.serializedProperty.arraySize;
                l.serializedProperty.arraySize++;
                l.index = index;
                var element = l.serializedProperty.GetArrayElementAtIndex(index);
                element.FindPropertyRelative("name").stringValue = string.Empty;
                element.FindPropertyRelative("value").stringValue = string.Empty;
            };
        }

        /// <summary>
        /// Unity's OnInspectorGUI method.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUIUtility.labelWidth = 160;

            EditorGUILayout.HelpBox("You specify the game server settings here.", MessageType.Info);

            GUILayout.Label("Game server settings", EditorStyles.boldLabel);

            var masterServerIp = serializedObject.FindProperty("masterServerIp");
            EditorGUILayout.PropertyField(masterServerIp, new GUIContent("Master server IP address"), GUILayout.MaxWidth(300));

            var masterServerPort = serializedObject.FindProperty("masterServerPort");
            EditorGUILayout.PropertyField(masterServerPort, new GUIContent("Master server port"), GUILayout.MaxWidth(210));

            var isStandalone = serializedObject.FindProperty("isStandalone");
            EditorGUILayout.PropertyField(isStandalone, new GUIContent("Is standalone"));

            if (isStandalone.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUIUtility.labelWidth = 170;

                var gameServerIp = serializedObject.FindProperty("gameServerIp");
                EditorGUILayout.PropertyField(gameServerIp, new GUIContent("Game server IP address"), GUILayout.MaxWidth(300));

                var gameServerPort = serializedObject.FindProperty("gameServerPort");
                EditorGUILayout.PropertyField(gameServerPort, new GUIContent("Game server port"), GUILayout.MaxWidth(210));

                var gameName = serializedObject.FindProperty("gameName");
                EditorGUILayout.PropertyField(gameName, new GUIContent("Name"), GUILayout.MaxWidth(300));

                var maxPlayers = serializedObject.FindProperty("maxPlayers");
                EditorGUILayout.PropertyField(maxPlayers, new GUIContent("Max players"), GUILayout.MaxWidth(190));

                var password = serializedObject.FindProperty("password");
                EditorGUILayout.PropertyField(password, new GUIContent("Password"), GUILayout.MaxWidth(300));

                EditorGUI.indentLevel--;
            }

            EditorGUIUtility.labelWidth = 160;

            var closeWhenEmpty = serializedObject.FindProperty("closeWhenEmpty");
            EditorGUILayout.PropertyField(closeWhenEmpty, new GUIContent("Close when empty"));

            var hideWhenFull = serializedObject.FindProperty("hideWhenFull");
            EditorGUILayout.PropertyField(hideWhenFull, new GUIContent("Hide when full"));

            var useNetworkManager = serializedObject.FindProperty("useNetworkManager");
            EditorGUILayout.PropertyField(useNetworkManager, new GUIContent("Use Network Manager"));

            propertiesList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}