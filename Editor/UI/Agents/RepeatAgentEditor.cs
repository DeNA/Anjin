// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using DeNA.Anjin.Agents;
using UnityEditor;
using UnityEngine;

namespace DeNA.Anjin.Editor.UI.Agents
{
    /// <summary>
    /// Editor GUI for RepeatAgent
    /// </summary>
    [CustomEditor(typeof(RepeatAgent))]
    public class RepeatAgentEditor : UnityEditor.Editor
    {
        private static readonly string s_description = L10n.Tr("Description");
        private static readonly string s_descriptionTooltip = L10n.Tr("Description about this Agent instance.");
        private static readonly string s_agent = L10n.Tr("Agent");
        private static readonly string s_agentTooltip = L10n.Tr("Agent to repeat execution");

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(RepeatAgent.description)),
                new GUIContent(s_description, s_descriptionTooltip));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(RepeatAgent.agent)),
                new GUIContent(s_agent, s_agentTooltip));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
