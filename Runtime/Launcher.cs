// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Attributes;
using DeNA.Anjin.Settings;
using DeNA.Anjin.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DeNA.Anjin
{
    /// <summary>
    /// Launch process when run on Unity editor.
    /// </summary>
    public static class Launcher
    {
        /// <summary>
        /// Run autopilot from Play Mode test.
        /// If an error is detected in running, it will be output to `LogError` and the test will fail.
        /// </summary>
        /// <param name="settings">Autopilot settings</param>
        public static async UniTask LaunchAutopilotAsync(AutopilotSettings settings)
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                throw new InvalidOperationException("Not support run from Edit Mode");
            }
#endif
            var state = AutopilotState.Instance;
            if (state.IsRunning)
            {
                throw new InvalidOperationException("Autopilot is already running");
            }

            state.launchFrom = LaunchType.PlayMode;
            state.settings = settings;
            LaunchAutopilot().Forget();

            await UniTask.WaitUntil(() => !state.IsRunning);
        }

        /// <summary>
        /// Run autopilot from Play Mode test.
        /// If an error is detected in running, it will be output to `LogError` and the test will fail.
        /// </summary>
        /// <param name="autopilotSettingsPath">Asset file path for autopilot settings. When running the player, it reads from <c>Resources</c></param>
        public static async UniTask LaunchAutopilotAsync(string autopilotSettingsPath)
        {
#if UNITY_EDITOR
            var settings = AssetDatabase.LoadAssetAtPath<AutopilotSettings>(autopilotSettingsPath);
#else
            var settings = Resources.Load<AutopilotSettings>(autopilotSettingsPath);
#endif
            await LaunchAutopilotAsync(settings);
        }

        /// <summary>
        /// Run autopilot
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        // ReSharper disable once Unity.IncorrectMethodSignature
        internal static async UniTaskVoid LaunchAutopilot()
        {
            var state = AutopilotState.Instance;
            if (!state.IsRunning)
            {
                return; // Normally play mode (not run autopilot)
            }

            ScreenshotStore.CleanDirectories();

            await CallAttachedInitializeOnLaunchAutopilotAttributeMethods();

            var autopilot = new GameObject(nameof(Autopilot)).AddComponent<Autopilot>();
            Object.DontDestroyOnLoad(autopilot);
        }

        private static async UniTask CallAttachedInitializeOnLaunchAutopilotAttributeMethods()
        {
            const BindingFlags MethodBindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (var methodInfo in AppDomain.CurrentDomain.GetAssemblies()
                         .SelectMany(x => x.GetTypes())
                         .SelectMany(x => x.GetMethods(MethodBindingFlags))
                         .Where(x => x.GetCustomAttributes(typeof(InitializeOnLaunchAutopilotAttribute), false).Any()))
            {
                switch (methodInfo.ReturnType.Name)
                {
                    case nameof(Task):
                        await (Task)methodInfo.Invoke(null, null);
                        break;
                    case nameof(UniTask):
                        await (UniTask)methodInfo.Invoke(null, null);
                        break;
                    default:
                        methodInfo.Invoke(null, null); // static method only
                        break;
                }
            }
        }
    }
}
