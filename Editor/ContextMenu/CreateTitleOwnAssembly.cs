// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using UnityEditor;
using UnityEditor.Experimental;
using UnityEngine;

namespace DeNA.Anjin.Editor.ContextMenu
{
    /// <summary>
    /// Create title own assembly
    /// </summary>
    public static class CreateTitleOwnAssembly
    {
        /// <summary>
        /// Create title own assembly
        /// </summary>
        [MenuItem("Assets/Create/Anjin/Title Own Assembly Folder")]
        public static void CreateTitleOwnAssemblyMenuItem()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                ScriptableObject.CreateInstance<DoCreateTitleOwnAssembly>(),
                "New Folder",
                EditorGUIUtility.IconContent(EditorResources.folderIconName).image as Texture2D,
                (string)null);
        }
    }
}
