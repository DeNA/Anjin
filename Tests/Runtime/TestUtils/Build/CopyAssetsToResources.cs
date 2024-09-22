// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.IO;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DeNA.Anjin.TestUtils.Build
{
    public class CopyAssetsToResources : IPrebuildSetup
    {
        private const string TestAssetsDirectory = "Packages/com.dena.anjin/Tests/TestAssets";
        internal const string ResourcesBaseDirectory = "Assets/com.dena.anjin";
        private static readonly string s_resourcesTestsDirectory = Path.Join(ResourcesBaseDirectory, "Resources/Tests");
        private static readonly string s_resourcesDirectory = Path.Join(s_resourcesTestsDirectory, "TestAssets");

        public static string GetAssetPath(string path)
        {
#if UNITY_EDITOR
            return Path.Combine(TestAssetsDirectory, path);
#else
            return Path.Combine("Tests/TestAssets", Path.GetFileNameWithoutExtension(path));
#endif
        }

        public void Setup()
        {
#if UNITY_EDITOR
            var src = Path.GetFullPath(TestAssetsDirectory);
            var dstDir = Path.GetFullPath(s_resourcesTestsDirectory);
            var dst = Path.GetFullPath(s_resourcesDirectory);

            if (!Directory.Exists(dstDir))
            {
                Directory.CreateDirectory(dstDir);
            }

            FileUtil.CopyFileOrDirectory(src, dst);
            // Note: copy asset files and meta files (guid)

            AssetDatabase.Refresh();
#endif
        }
    }
}
