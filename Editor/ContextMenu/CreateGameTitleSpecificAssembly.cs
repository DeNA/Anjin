// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using UnityEditor;
using UnityEditor.Experimental;
using UnityEngine;

namespace DeNA.Anjin.Editor.ContextMenu
{
    /// <summary>
    /// Create game title specific assembly
    /// </summary>
    public static class CreateGameTitleSpecificAssembly
    {
        /// <summary>
        /// Create game title specific assembly
        /// </summary>
        [MenuItem("Assets/Create/Anjin/Game Title Specific Assembly Folder")]
        public static void CreateGameTitleSpecificAssemblyMenuItem()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                ScriptableObject.CreateInstance<DoCreateGameTitleSpecificAssembly>(),
                "New Folder",
                EditorGUIUtility.IconContent(EditorResources.folderIconName).image as Texture2D,
                (string)null);
        }
    }
}
