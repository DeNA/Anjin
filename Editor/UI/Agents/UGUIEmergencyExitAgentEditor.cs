// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using DeNA.Anjin.Agents;
using UnityEditor;
using UnityEngine;

namespace DeNA.Anjin.Editor.UI.Agents
{
    /// <summary>
    /// Editor GUI for UGUIEmergencyExitAgent
    /// </summary>
    [CustomEditor(typeof(UGUIEmergencyExitAgent))]
    public class UGUIEmergencyExitAgentEditor : UnityEditor.Editor
    {
        private const float SpacerPixels = 10f;

        // @formatter:off
        private static readonly string s_description = L10n.Tr("Description");
        private static readonly string s_descriptionTooltip = L10n.Tr("Description about this agent instance");
        private SerializedProperty _descriptionProp;
        private GUIContent _descriptionLabel;

        private static readonly string s_intervalMillis = L10n.Tr("Interval [ms]");
        private static readonly string s_intervalMillisTooltip = L10n.Tr("Interval in milliseconds to check for the appearance of the EmergencyExitAnnotation component.");
        private SerializedProperty _intervalMillisProp;
        private GUIContent _intervalMillisLabel;
        // @formatter:on

        private void OnEnable()
        {
            Initialize();
        }

        private void Initialize()
        {
            _descriptionProp = serializedObject.FindProperty(nameof(UGUIEmergencyExitAgent.description));
            _descriptionLabel = new GUIContent(s_description, s_descriptionTooltip);

            _intervalMillisProp = serializedObject.FindProperty(nameof(UGUIEmergencyExitAgent.intervalMillis));
            _intervalMillisLabel = new GUIContent(s_intervalMillis, s_intervalMillisTooltip);
        }

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_descriptionProp, _descriptionLabel);
            GUILayout.Space(SpacerPixels);

            EditorGUILayout.PropertyField(_intervalMillisProp, _intervalMillisLabel);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
