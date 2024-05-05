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
    [CustomEditor(typeof(FileLogger))]
    public class FileLoggerEditor : UnityEditor.Editor
    {
        private static readonly string s_description = L10n.Tr("Description");
        private static readonly string s_descriptionTooltip = L10n.Tr("Description about this logger instance");

        private static readonly string s_outputPath = L10n.Tr("Output File Path");
        private static readonly string s_outputPathTooltip = L10n.Tr("Log output file path. Specify relative path from project root or absolute path.");

        private static readonly string s_filterLogType = L10n.Tr("Filter LogType");
        private static readonly string s_filterLogTypeTooltip = L10n.Tr("To selective enable debug log message");

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(FileLogger.description)),
                new GUIContent(s_description, s_descriptionTooltip));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(FileLogger.outputPath)),
                new GUIContent(s_outputPath, s_outputPathTooltip));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(FileLogger.filterLogType)),
                new GUIContent(s_filterLogType, s_filterLogTypeTooltip));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
