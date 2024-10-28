// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.IO;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DeNA.Anjin.TestUtils.Build
{
    public class CleanupResources : IPostBuildCleanup
    {
        public void Cleanup()
        {
#if UNITY_EDITOR
            var path = Path.GetFullPath(CopyAssetsToResources.ResourcesBaseDirectory);
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            var meta = Path.GetFullPath(CopyAssetsToResources.ResourcesBaseDirectory + ".meta");
            if (File.Exists(meta))
            {
                File.Delete(meta);
            }

            AssetDatabase.Refresh();
#endif
        }
    }
}
