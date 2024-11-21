// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.IO;

namespace DeNA.Anjin.Utilities
{
    internal static class PathUtils
    {
        /// <summary>
        /// Returns the absolute path.
        /// </summary>
        /// <param name="path">The file or directory for which to obtain the absolute path.</param>
        /// <param name="basePath">The base path to use when a relative path is specified in path.</param>
        /// <returns>Absolute path.</returns>
        public static string GetAbsolutePath(string path, string basePath)
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

            return Path.Combine(basePath, path);
        }
    }
}
