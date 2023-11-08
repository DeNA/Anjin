// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using DeNA.Anjin.Reporters;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace DeNA.Anjin.Editor.UI.Reporters
{
    /// <summary>
    /// Editor for <c cref="CompositeReporter" />
    /// </summary>
    [CustomEditor(typeof(CompositeReporter))]
    public class CompositeReporterEditor : UnityEditor.Editor
    {
        private SerializedProperty _reportersProp;
        private ReorderableList _reorderableList;
        private static readonly string s_reporters = L10n.Tr("Reporters");
        private GUIContent _reportersGUIContent;
        private static readonly string s_reporter = L10n.Tr("Reporter");
        private GUIContent _reporterGUIContent;


        private void OnEnable()
        {
            Initialize();
        }


        private void Initialize()
        {
            _reportersProp = serializedObject.FindProperty(nameof(CompositeReporter.reporters));
            _reportersGUIContent = new GUIContent(s_reporters);
            _reporterGUIContent = new GUIContent(s_reporter);
            _reorderableList = new ReorderableList(serializedObject, _reportersProp)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, _reportersGUIContent),
                elementHeightCallback = _ => EditorGUIUtility.singleLineHeight,
                // XXX: Dont use discarded parameter to treat Unity 2019.x
                // ReSharper disable UnusedParameter.Local
                drawElementCallback = (rect, index, active, focused) =>
                // ReSharper restore UnusedParameter.Local
                {
                    var elemProp = _reportersProp.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(rect, elemProp, _reporterGUIContent);
                }
            };
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            _reorderableList.DoLayoutList();
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
