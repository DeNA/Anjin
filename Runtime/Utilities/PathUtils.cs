// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.IO;
using UnityEngine;

namespace DeNA.Anjin.Utilities
{
    internal static class PathUtils
    {
        /// <summary>
        /// Returns the absolute path.
        /// In the Editor, it returns a path from the project's root.
        /// In the Player, it returns a path from the <c>Application.persistentDataPath</c>.
        /// </summary>
        /// <param name="path">The file or directory for which to obtain absolute path information.</param>
        public static string GetAbsolutePath(string path)
        {
#if UNITY_2021_2_OR_NEWER
            if (Path.IsPathFullyQualified(path))
            {
                return path;
            }
#else
            if (Path.IsPathRooted(path)) // This is inaccurate but rarely a problem.
            {
                return path;
            }
#endif

            return Application.isEditor
                ? Path.GetFullPath(path)
                : Path.Combine(Application.persistentDataPath, path);
        }
    }
}
