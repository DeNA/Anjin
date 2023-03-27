// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.IO;
using UnityEngine;

namespace DeNA.Anjin.Utilities
{
    /// <summary>
    /// Store screenshots by Agents
    /// </summary>
    public static class ScreenshotStore
    {
        private static string StoreRootPath => Path.Combine(Application.persistentDataPath, "Anjin");

        /// <summary>
        /// Clean screenshot store root directory
        /// for call when launch autopilot
        /// </summary>
        public static void CleanDirectories()
        {
            if (Directory.Exists(StoreRootPath))
            {
                Directory.Delete(StoreRootPath, true);
            }

            Directory.CreateDirectory(StoreRootPath);
        }

        /// <summary>
        /// Create and return screenshots store path for each Agent
        /// 
        /// Path is <c>Application.persistentDataPath</c>/Anjin/yyyyMMddHHmmss_AgentName/
        /// See https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html
        /// </summary>
        /// <param name="time">Timestamp when start agent</param>
        /// <param name="agentName">Agent name (optional)</param>
        /// <returns></returns>
        public static string CreateDirectory(DateTime time, string agentName = null)
        {
            var timestamp = time.ToString("yyyyMMddHHmmss");
            string path;
            if (agentName == null)
            {
                path = Path.Combine(StoreRootPath, timestamp);
            }
            else
            {
                path = Path.Combine(StoreRootPath, $"{timestamp}_{agentName}");
            }

            Directory.CreateDirectory(path);
            return path;
        }

        /// <summary>
        /// Create and return screenshots store path for each Agent
        /// 
        /// Path is <c>Application.persistentDataPath</c>/Anjin/yyyyMMddHHmmss_AgentName/
        /// See https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html
        /// </summary>
        /// <param name="agentName">Agent name (optional)</param>
        /// <returns></returns>
        public static string CreateDirectory(string agentName = null)
        {
            return CreateDirectory(DateTime.Now, agentName);
        }
    }
}
