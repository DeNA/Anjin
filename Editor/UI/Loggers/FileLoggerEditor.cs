// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using DeNA.Anjin.Loggers;
using UnityEditor;
using UnityEngine;

namespace DeNA.Anjin.Editor.UI.Loggers
{
    /// <summary>
    /// Editor GUI for FileLogger.
    /// </summary>
    [CustomEditor(typeof(FileLoggerAsset))]
    public class FileLoggerEditor : UnityEditor.Editor
    {
        // @formatter:off
        private static readonly string s_description = L10n.Tr("Description");
        private static readonly string s_descriptionTooltip = L10n.Tr("Description about this Logger instance.");

        private static readonly string s_outputPath = L10n.Tr("Output File Path");
        private static readonly string s_outputPathTooltip = L10n.Tr("Output path for log file path. When a relative path is specified, relative to the AutopilotSettings.outputRootPath.");

        private static readonly string s_filterLogType = L10n.Tr("Filter LogType");
        private static readonly string s_filterLogTypeTooltip = L10n.Tr("To selective enable debug log message");

        private static readonly string s_timestamp = L10n.Tr("Timestamp");
        private static readonly string s_timestampTooltip = L10n.Tr("Output timestamp to log entities");
        // @formatter:on

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(FileLoggerAsset.description)),
                new GUIContent(s_description, s_descriptionTooltip));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(FileLoggerAsset.outputPath)),
                new GUIContent(s_outputPath, s_outputPathTooltip));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(FileLoggerAsset.filterLogType)),
                new GUIContent(s_filterLogType, s_filterLogTypeTooltip));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(FileLoggerAsset.timestamp)),
                new GUIContent(s_timestamp, s_timestampTooltip));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
