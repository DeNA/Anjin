// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using DeNA.Anjin.Loggers;
using UnityEditor;
using UnityEngine;

namespace DeNA.Anjin.Editor.UI.Loggers
{
    /// <summary>
    /// Editor GUI for CompositeLogger.
    /// </summary>
    [CustomEditor(typeof(CompositeLoggerAsset))]
    public class CompositeLoggerEditor : UnityEditor.Editor
    {
        private static readonly string s_description = L10n.Tr("Description");
        private static readonly string s_descriptionTooltip = L10n.Tr("Description about this Logger instance.");

        private static readonly string s_loggers = L10n.Tr("Loggers");
        private static readonly string s_loggersTooltip = L10n.Tr("Loggers to delegates");

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(CompositeLoggerAsset.description)),
                new GUIContent(s_description, s_descriptionTooltip));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(CompositeLoggerAsset.loggerAssets)),
                new GUIContent(s_loggers, s_loggersTooltip));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
