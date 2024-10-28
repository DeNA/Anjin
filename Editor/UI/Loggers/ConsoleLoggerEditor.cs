// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using DeNA.Anjin.Loggers;
using UnityEditor;
using UnityEngine;

namespace DeNA.Anjin.Editor.UI.Loggers
{
    /// <summary>
    /// Editor GUI for ConsoleLogger.
    /// </summary>
    [CustomEditor(typeof(ConsoleLoggerAsset))]
    public class ConsoleLoggerEditor : UnityEditor.Editor
    {
        private static readonly string s_description = L10n.Tr("Description");
        private static readonly string s_descriptionTooltip = L10n.Tr("Description about this Logger instance");

        private static readonly string s_filterLogType = L10n.Tr("Filter LogType");
        private static readonly string s_filterLogTypeTooltip = L10n.Tr("To selective enable debug log message");

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(ConsoleLoggerAsset.description)),
                new GUIContent(s_description, s_descriptionTooltip));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(ConsoleLoggerAsset.filterLogType)),
                new GUIContent(s_filterLogType, s_filterLogTypeTooltip));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
