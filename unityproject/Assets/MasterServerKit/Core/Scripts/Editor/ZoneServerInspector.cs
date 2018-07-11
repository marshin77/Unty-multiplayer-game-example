// Copyright (C) 2016-2017 Spelltwine Games. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MasterServerKit
{
    /// <summary>
    /// The custom inspector for the ZoneServer class.
    /// </summary>
    [CustomEditor(typeof(ZoneServer), true)]
    public class ZoneServerInspector : Editor
    {
        /// <summary>
        /// The list of properties of this zone server.
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

            EditorGUIUtility.labelWidth = 150;

            EditorGUILayout.HelpBox("You specify the IP address and port of the master server here. The zone server " +
                "will automatically register itself in it upon launch.", MessageType.Info);

            GUILayout.Label("Master server", EditorStyles.boldLabel);

            var masterServerIp = serializedObject.FindProperty("masterServerIp");
            EditorGUILayout.PropertyField(masterServerIp, new GUIContent("IP address"), GUILayout.MaxWidth(300));

            var masterServerPort = serializedObject.FindProperty("masterServerPort");
            EditorGUILayout.PropertyField(masterServerPort, new GUIContent("Port"), GUILayout.MaxWidth(210));

            GUILayout.Space(10);
            EditorGUILayout.HelpBox("You specify the zone server settings here.", MessageType.Info);

            GUILayout.Label("Zone server", EditorStyles.boldLabel);

            var zoneServerIp = serializedObject.FindProperty("zoneServerIp");
            EditorGUILayout.PropertyField(zoneServerIp, new GUIContent("IP address"), GUILayout.MaxWidth(300));

            var zoneServerPort = serializedObject.FindProperty("zoneServerPort");
            EditorGUILayout.PropertyField(zoneServerPort, new GUIContent("Port"), GUILayout.MaxWidth(210));

            propertiesList.DoLayoutList();

            GUILayout.Space(10);
            EditorGUILayout.HelpBox("You specify the game server settings here.", MessageType.Info);

            GUILayout.Label("Game server", EditorStyles.boldLabel);

            var binaryPath = serializedObject.FindProperty("gameServerBinaryPath");
            EditorGUILayout.PropertyField(binaryPath, new GUIContent("Path to binary"), GUILayout.MaxWidth(450));

            var maxGameServers = serializedObject.FindProperty("maxGameServers");
            EditorGUILayout.PropertyField(maxGameServers, new GUIContent("Maximum instances"), GUILayout.MaxWidth(190));

            var numGameServers = serializedObject.FindProperty("numPreSpawnedGameServers");
            EditorGUILayout.PropertyField(numGameServers, new GUIContent("Pre-spawned instances"), GUILayout.MaxWidth(190));

            var spawnInBatchMode = serializedObject.FindProperty("spawnGameServersInBatchMode");
            EditorGUILayout.PropertyField(spawnInBatchMode, new GUIContent("Spawn in batch mode"), GUILayout.MaxWidth(190));

            var defaultGameName = serializedObject.FindProperty("defaultGameName");
            EditorGUILayout.PropertyField(defaultGameName, new GUIContent("Default game name"), GUILayout.MaxWidth(300));

            var defaultGameMaxPlayers = serializedObject.FindProperty("defaultGameMaxPlayers");
            EditorGUILayout.PropertyField(defaultGameMaxPlayers, new GUIContent("Default game players"), GUILayout.MaxWidth(190));

            serializedObject.ApplyModifiedProperties();
        }
    }
}