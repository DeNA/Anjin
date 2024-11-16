// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using DeNA.Anjin.Agents;
using UnityEditor;
using UnityEngine;

namespace DeNA.Anjin.Editor.UI.Agents
{
    /// <summary>
    /// Editor GUI for TerminateAgent.
    /// </summary>
    [CustomEditor(typeof(TerminateAgent))]
    public class TerminateAgentEditor : UnityEditor.Editor
    {
        // @formatter:off
        private static readonly string s_description = L10n.Tr("Description");
        private static readonly string s_descriptionTooltip = L10n.Tr("Description about this agent instance");

        private static readonly string s_exitCode = L10n.Tr("Exit Code");
        private static readonly string s_exitCodeTooltip = L10n.Tr("Exit code used when terminated by this Agent.");
        private static readonly string s_customExitCode = L10n.Tr("Custom Exit Code");
        private static readonly string s_customExitCodeTooltip = L10n.Tr("Input exit code by integer value.");
        private static readonly string s_exitMessage = L10n.Tr("Message");
        private static readonly string s_exitMessageTooltip = L10n.Tr("Message sent by the Reporter when terminated.");
        // @formatter:on

        private const float SpacerPixels = 10f;

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(TerminateAgent.description)),
                new GUIContent(s_description, s_descriptionTooltip));
            GUILayout.Space(SpacerPixels);

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(TerminateAgent.exitCode)),
                new GUIContent(s_exitCode, s_exitCodeTooltip));
            EditorGUI.BeginDisabledGroup(((TerminateAgent)target).exitCode != ExitCodeByTerminateAgent.Custom);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(TerminateAgent.customExitCode)),
                new GUIContent(s_customExitCode, s_customExitCodeTooltip));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(TerminateAgent.exitMessage)),
                new GUIContent(s_exitMessage, s_exitMessageTooltip));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
