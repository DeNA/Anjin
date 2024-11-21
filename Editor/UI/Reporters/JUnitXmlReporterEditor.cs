// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using DeNA.Anjin.Reporters;
using UnityEditor;
using UnityEngine;

namespace DeNA.Anjin.Editor.UI.Reporters
{
    /// <summary>
    /// Editor for <c cref="JUnitXmlReporter"/>.
    /// </summary>
    [CustomEditor(typeof(JUnitXmlReporter))]
    public class JUnitXmlReporterEditor : UnityEditor.Editor
    {
        private const float SpacerPixels = 10f;

        // @formatter:off
        private static readonly string s_description = L10n.Tr("Description");
        private static readonly string s_descriptionTooltip = L10n.Tr("Description about this Reporter instance");
        private GUIContent _descriptionLabel;
        private SerializedProperty _descriptionProp;

        private static readonly string s_outputPath = L10n.Tr("Output File Path");
        private static readonly string s_outputPathTooltip = L10n.Tr("Output path for JUnit XML format file. When a relative path is specified, relative to the AutopilotSettings.outputRootPath.");
        private GUIContent _outputPathLabel;
        private SerializedProperty _outputPathProp;
        // @formatter:on

        private void OnEnable()
        {
            Initialize();
        }

        private void Initialize()
        {
            _descriptionProp = serializedObject.FindProperty(nameof(JUnitXmlReporter.description));
            _descriptionLabel = new GUIContent(s_description, s_descriptionTooltip);

            _outputPathProp = serializedObject.FindProperty(nameof(JUnitXmlReporter.outputPath));
            _outputPathLabel = new GUIContent(s_outputPath, s_outputPathTooltip);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_descriptionProp, _descriptionLabel);
            GUILayout.Space(SpacerPixels);

            EditorGUILayout.PropertyField(_outputPathProp, _outputPathLabel);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
