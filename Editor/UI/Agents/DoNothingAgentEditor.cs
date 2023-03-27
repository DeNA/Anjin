// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using DeNA.Anjin.Agents;
using UnityEditor;
using UnityEngine;

namespace DeNA.Anjin.Editor.UI.Agents
{
    /// <summary>
    /// Editor GUI for DoNothingAgent
    /// </summary>
    [CustomEditor(typeof(DoNothingAgent))]
    public class DoNothingAgentEditor : UnityEditor.Editor
    {
        private static readonly string s_description = L10n.Tr("Description");
        private static readonly string s_descriptionTooltip = L10n.Tr("Description about this agent instance");
        private static readonly string s_lifespanSec = L10n.Tr("Lifespan Sec");

        private static readonly string s_lifespanSecTooltip =
            L10n.Tr("Agent running lifespan [sec]. When specified zero, so unlimited running");

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(DoNothingAgent.description)),
                new GUIContent(s_description, s_descriptionTooltip));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(DoNothingAgent.lifespanSec)),
                new GUIContent(s_lifespanSec, s_lifespanSecTooltip));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
