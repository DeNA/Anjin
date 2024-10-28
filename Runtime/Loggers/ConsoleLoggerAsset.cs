// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using DeNA.Anjin.Attributes;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DeNA.Anjin.Loggers
{
    /// <summary>
    /// A Logger that outputs to a console.
    /// Logger using <c>Debug.unityLogger</c>
    /// </summary>
    [CreateAssetMenu(fileName = "New ConsoleLogger", menuName = "Anjin/Console Logger", order = 71)]
    public class ConsoleLoggerAsset : AbstractLoggerAsset
    {
        /// <summary>
        /// To selective enable debug log message.
        /// </summary>
        public LogType filterLogType = LogType.Log;

        private ILogger _logger;

        /// <inheritdoc />
        public override ILogger Logger
        {
            get
            {
                if (_logger != null)
                {
                    return _logger;
                }

                _logger = new Logger(Debug.unityLogger.logHandler) { filterLogType = filterLogType };
                return _logger;
            }
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            // Nothing to dispose.
        }

        [InitializeOnLaunchAutopilot(InitializeOnLaunchAutopilotAttribute.InitializeLoggerOrder)]
        public static void ResetLoggers()
        {
            // Reset runtime instances
            var loggerAssets = FindObjectsOfType<ConsoleLoggerAsset>();
            foreach (var current in loggerAssets)
            {
                current._logger = null;
            }
#if UNITY_EDITOR
            // Reset asset files (in Editor only)
            foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(ConsoleLoggerAsset)}"))
            {
                var so = AssetDatabase.LoadAssetAtPath<ConsoleLoggerAsset>(AssetDatabase.GUIDToAssetPath(guid));
                so._logger = null;
            }
#endif
        }
    }
}
