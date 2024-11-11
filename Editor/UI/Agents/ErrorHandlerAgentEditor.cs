// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using DeNA.Anjin.Agents;
using UnityEditor;
using UnityEngine;

namespace DeNA.Anjin.Editor.UI.Agents
{
    /// <summary>
    /// Editor GUI for ErrorHandlerAgent
    /// </summary>
    [CustomEditor(typeof(ErrorHandlerAgent))]
    public class ErrorHandlerAgentEditor : UnityEditor.Editor
    {
        private const float SpacerPixels = 10f;

        // @formatter:off
        private static readonly string s_description = L10n.Tr("Description");
        private static readonly string s_descriptionTooltip = L10n.Tr("Description about this Agent instance");
        private SerializedProperty _descriptionProp;
        private GUIContent _descriptionLabel;

        private static readonly string s_handleException = L10n.Tr("Handle Exception");
        private static readonly string s_handleExceptionTooltip = L10n.Tr("Specify an Autopilot terminates or only reports when an Exception is detected in the log");
        private SerializedProperty _handleExceptionProp;
        private GUIContent _handleExceptionLabel;

        private static readonly string s_handleError = L10n.Tr("Handle Error");
        private static readonly string s_handleErrorTooltip = L10n.Tr("Specify an Autopilot terminates or only reports when an Error is detected in the log");
        private SerializedProperty _handleErrorProp;
        private GUIContent _handleErrorLabel;

        private static readonly string s_handleAssert = L10n.Tr("Handle Assert");
        private static readonly string s_handleAssertTooltip = L10n.Tr("Specify an Autopilot terminates or only reports when an Assert is detected in the log");
        private SerializedProperty _handleAssertProp;
        private GUIContent _handleAssertLabel;

        private static readonly string s_handleWarning = L10n.Tr("Handle Warning");
        private static readonly string s_handleWarningTooltip = L10n.Tr("Specify an Autopilot terminates or only reports when an Warning is detected in the log");
        private SerializedProperty _handleWarningProp;
        private GUIContent _handleWarningLabel;

        private static readonly string s_ignoreMessages = L10n.Tr("Ignore Messages");
        private static readonly string s_ignoreMessagesTooltip = L10n.Tr("Log messages containing the specified strings will be ignored from the stop condition. Regex is also available; escape is a single backslash (\\).");
        private SerializedProperty _ignoreMessagesProp;
        private GUIContent _ignoreMessagesLabel;
        // @formatter:on

        private void OnEnable()
        {
            Initialize();
        }

        private void Initialize()
        {
            _descriptionProp = serializedObject.FindProperty(nameof(ErrorHandlerAgent.description));
            _descriptionLabel = new GUIContent(s_description, s_descriptionTooltip);

            _handleExceptionProp = serializedObject.FindProperty(nameof(ErrorHandlerAgent.handleException));
            _handleExceptionLabel = new GUIContent(s_handleException, s_handleExceptionTooltip);

            _handleErrorProp = serializedObject.FindProperty(nameof(ErrorHandlerAgent.handleError));
            _handleErrorLabel = new GUIContent(s_handleError, s_handleErrorTooltip);

            _handleAssertProp = serializedObject.FindProperty(nameof(ErrorHandlerAgent.handleAssert));
            _handleAssertLabel = new GUIContent(s_handleAssert, s_handleAssertTooltip);

            _handleWarningProp = serializedObject.FindProperty(nameof(ErrorHandlerAgent.handleWarning));
            _handleWarningLabel = new GUIContent(s_handleWarning, s_handleWarningTooltip);

            _ignoreMessagesProp = serializedObject.FindProperty(nameof(ErrorHandlerAgent.ignoreMessages));
            _ignoreMessagesLabel = new GUIContent(s_ignoreMessages, s_ignoreMessagesTooltip);
        }

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_descriptionProp, _descriptionLabel);
            GUILayout.Space(SpacerPixels);

            EditorGUILayout.PropertyField(_handleExceptionProp, _handleExceptionLabel);
            EditorGUILayout.PropertyField(_handleErrorProp, _handleErrorLabel);
            EditorGUILayout.PropertyField(_handleAssertProp, _handleAssertLabel);
            EditorGUILayout.PropertyField(_handleWarningProp, _handleWarningLabel);
            GUILayout.Space(SpacerPixels);

            EditorGUILayout.PropertyField(_ignoreMessagesProp, _ignoreMessagesLabel);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
