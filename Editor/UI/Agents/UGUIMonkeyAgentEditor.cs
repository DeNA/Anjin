// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Diagnostics.CodeAnalysis;
using DeNA.Anjin.Agents;
using UnityEditor;
using UnityEngine;

namespace DeNA.Anjin.Editor.UI.Agents
{
    /// <summary>
    /// Editor GUI for UGUIMonkeyAgent
    /// </summary>
    [CustomEditor(typeof(UGUIMonkeyAgent))]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase")]
    public class UGUIMonkeyAgentEditor : UnityEditor.Editor
    {
        private static readonly string s_description = L10n.Tr("Description");
        private static readonly string s_descriptionTooltip = L10n.Tr("Description about this agent instance");
        private static readonly string s_lifespanSec = L10n.Tr("Lifespan Sec");

        private static readonly string s_lifespanSecTooltip =
            L10n.Tr("Agent running lifespan [sec]. When specified zero, so unlimited running");

        private static readonly string s_delayMillis = L10n.Tr("Delay Millis");
        private static readonly string s_delayMillisTooltip = L10n.Tr("Delay time between random operations [ms]");

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UGUIMonkeyAgent.description)),
                new GUIContent(s_description, s_descriptionTooltip));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UGUIMonkeyAgent.lifespanSec)),
                new GUIContent(s_lifespanSec, s_lifespanSecTooltip));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(UGUIMonkeyAgent.delayMillis)),
                new GUIContent(s_delayMillis, s_delayMillisTooltip));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
