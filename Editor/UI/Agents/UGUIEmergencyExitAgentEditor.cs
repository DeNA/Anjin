// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using DeNA.Anjin.Agents;
using UnityEditor;
using UnityEngine;

namespace DeNA.Anjin.Editor.UI.Agents
{
    /// <summary>
    /// Editor GUI for DoNothingAgent
    /// </summary>
    [CustomEditor(typeof(UGUIEmergencyExitAgent))]
    public class UGUIEmergencyExitAgentEditor : UnityEditor.Editor
    {
        private static readonly string s_description = L10n.Tr("Description");
        private static readonly string s_descriptionTooltip = L10n.Tr("Description about this agent instance");

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UGUIEmergencyExitAgent.description)),
                new GUIContent(s_description, s_descriptionTooltip));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
