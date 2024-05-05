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
    [CustomEditor(typeof(ConsoleLogger))]
    public class ConsoleLoggerEditor : UnityEditor.Editor
    {
        private static readonly string s_description = L10n.Tr("Description");
        private static readonly string s_descriptionTooltip = L10n.Tr("Description about this logger instance");

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(ConsoleLogger.description)),
                new GUIContent(s_description, s_descriptionTooltip));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
