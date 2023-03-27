// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using DeNA.Anjin.Settings;
using UnityEditor;
using UnityEngine;

namespace DeNA.Anjin.Editor.UI.Settings
{
    /// <summary>
    /// Scene picker
    /// </summary>
    [CustomPropertyDrawer(typeof(ScenePickerAttribute))]
    public class ScenePicker : PropertyDrawer
    {
        /// <inheritdoc/>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                if (attribute is ScenePickerAttribute scenePickerAttribute &&
                    !string.IsNullOrEmpty(scenePickerAttribute.Label))
                {
                    label.text = scenePickerAttribute.Label;
                }

                var oldScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(property.stringValue);
                var newScene = EditorGUI.ObjectField(position, label, oldScene, typeof(SceneAsset), true) as SceneAsset;
                property.stringValue = AssetDatabase.GetAssetPath(newScene);
            }
            else
            {
                Debug.LogError($"ScenePickerAttribute was attached not string field `{label}`");
            }
        }
    }
}
