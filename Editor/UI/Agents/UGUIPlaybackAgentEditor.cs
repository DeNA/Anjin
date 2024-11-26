// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Diagnostics.CodeAnalysis;
using DeNA.Anjin.Agents;
using UnityEditor;
using UnityEngine;

namespace DeNA.Anjin.Editor.UI.Agents
{
    /// <summary>
    /// Editor GUI for UGUIPlaybackAgent
    /// </summary>
    [CustomEditor(typeof(UGUIPlaybackAgent))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase")]
    public class UGUIPlaybackAgentEditor : UnityEditor.Editor
    {
        private static readonly string s_description = L10n.Tr("Description");
        private static readonly string s_descriptionTooltip = L10n.Tr("Description about this Agent instance.");
        private static readonly string s_recordedJson = L10n.Tr("Recorded JSON file");
        private static readonly string s_recordedJsonTooltip = L10n.Tr("JSON file recorded by AutomatedQA package");

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UGUIPlaybackAgent.description)),
                new GUIContent(s_description, s_descriptionTooltip));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UGUIPlaybackAgent.recordedJson)),
                new GUIContent(s_recordedJson, s_recordedJsonTooltip));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
