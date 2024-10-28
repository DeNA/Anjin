// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.IO;
using DeNA.Anjin.Editor.Definitions;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
#if !UNITY_6000_0_OR_NEWER
using System.Reflection;
using UnityEngine;
#endif

namespace DeNA.Anjin.Editor.ContextMenu
{
    /// <inheritdoc/>
    public class DoCreateGameTitleSpecificAssembly : EndNameEditAction
    {
        /// <inheritdoc/>
        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            var moduleName = Path.GetFileName(pathName);
            AssetDatabase.CreateFolder(Path.GetDirectoryName(pathName), moduleName);
            CreateAssemblyDefinitionFile(pathName);
        }

        private static void CreateAssemblyDefinitionFile(string pathName)
        {
            var assemblyName = Path.GetFileName(pathName);
            var asmdef = new AssemblyDefinition
            {
                name = assemblyName,
                autoReferenced = false,
                defineConstraints = new[] { "UNITY_INCLUDE_TESTS || DENA_AUTOPILOT_ENABLE" },
                references = new[] { "DeNA.Anjin", "UniTask" }
            };

            var asmdefPath = Path.Combine(pathName, $"{assemblyName}.asmdef");
            CreateScriptAssetWithContent(asmdefPath, EditorJsonUtility.ToJson(asmdef));
        }

        private static void CreateScriptAssetWithContent(string path, string content)
        {
#if UNITY_6000_0_OR_NEWER
            ProjectWindowUtil.CreateScriptAssetWithContent(path, content);
#else
            var projectWindowUtilType = typeof(ProjectWindowUtil);
            var createScriptAssetWithContentMethod = projectWindowUtilType.GetMethod("CreateScriptAssetWithContent",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (createScriptAssetWithContentMethod == null)
            {
                Debug.LogError("Can not invoke UnityEditor.ProjectWindowUtil.CreateScriptAssetWithContent()");
                return;
            }

            createScriptAssetWithContentMethod.Invoke(null, new object[] { path, content });
#endif
        }
    }
}
