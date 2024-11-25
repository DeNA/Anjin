// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.IO;
using DeNA.Anjin.Settings;
using DeNA.Anjin.Settings.ArgumentCapture;

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

        /// <summary>
        /// Constructs the output file path and creates directories.
        /// </summary>
        /// <param name="outputPathField">outputPath field</param>
        /// <param name="arg">Commandline argument</param>
        /// <returns>Output file absolute path or null if not specified.</returns>
        public static string GetOutputPath(string outputPathField, IArgument<string> arg)
        {
            string path;
            if (arg.IsCaptured())
            {
                path = arg.Value();
            }
            else if (!string.IsNullOrEmpty(outputPathField))
            {
                // ReSharper disable once PossibleNullReferenceException
                var outputRootPath = AutopilotState.Instance.settings.OutputRootPath;
                path = GetAbsolutePath(outputPathField, outputRootPath);
            }
            else
            {
                return null;
            }

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return path;
        }
    }
}
