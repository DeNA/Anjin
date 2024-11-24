// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using DeNA.Anjin.Agents;
using UnityEditor;
using UnityEngine;

namespace DeNA.Anjin.Editor.UI.Agents
{
    /// <summary>
    /// Editor GUI for SerialCompositeAgent
    /// </summary>
    [CustomEditor(typeof(SerialCompositeAgent))]
    public class SerialCompositeAgentEditor : UnityEditor.Editor
    {
        private static readonly string s_description = L10n.Tr("Description");
        private static readonly string s_descriptionTooltip = L10n.Tr("Description about this Agent instance.");
        private static readonly string s_agents = L10n.Tr("Agents");
        private static readonly string s_agentsTooltip = L10n.Tr("Agents to serial execution");

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SerialCompositeAgent.description)),
                new GUIContent(s_description, s_descriptionTooltip));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(SerialCompositeAgent.agents)),
                new GUIContent(s_agents, s_agentsTooltip), true);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
