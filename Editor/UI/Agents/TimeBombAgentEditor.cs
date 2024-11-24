// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using DeNA.Anjin.Agents;
using UnityEditor;
using UnityEngine;

namespace DeNA.Anjin.Editor.UI.Agents
{
    /// <summary>
    /// Editor GUI for TimeBombAgent
    /// </summary>
    [CustomEditor(typeof(TimeBombAgent))]
    public class TimeBombAgentEditor : UnityEditor.Editor
    {
        private static readonly string s_description = L10n.Tr("Description");
        private static readonly string s_descriptionTooltip = L10n.Tr("Description about this Agent instance.");
        private static readonly string s_agent = L10n.Tr("Agent");

        private static readonly string s_agentTooltip =
            L10n.Tr("Working Agent. If this Agent exits first, the TimeBombAgent will fail.");

        private static readonly string s_defuseMessage = L10n.Tr("Defuse Message");

        private static readonly string s_defuseMessageTooltip =
            L10n.Tr("Defuse the time bomb when this message comes to the log first. Can specify regex.");

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(TimeBombAgent.description)),
                new GUIContent(s_description, s_descriptionTooltip));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(TimeBombAgent.agent)),
                new GUIContent(s_agent, s_agentTooltip));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(TimeBombAgent.defuseMessage)),
                new GUIContent(s_defuseMessage, s_defuseMessageTooltip));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
