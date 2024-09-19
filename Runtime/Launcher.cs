// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

#if UNITY_EDITOR

using System;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Attributes;
using DeNA.Anjin.Settings;
using DeNA.Anjin.Utilities;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DeNA.Anjin
{
    /// <summary>
    /// Launch process when run on Unity editor.
    /// </summary>
    public static class Launcher
    {
        /// <summary>
        /// Reset event handlers even if domain reload is off
        /// <see href="https://docs.unity3d.com/ja/current/Manual/ConfigurableEnterPlayMode.html"/>
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void ResetEventHandlers()
        {
            EditorApplication.playModeStateChanged -= OnChangePlayModeState;
        }

        /// <summary>
        /// Run autopilot
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        // ReSharper disable once Unity.IncorrectMethodSignature
        public static async UniTaskVoid Run()
        {
            var state = AutopilotState.Instance;
            if (!state.IsRunning)
            {
                return; // Normally play mode (not run autopilot)
            }

            if (!state.IsLaunchFromPlayMode)
            {
                EditorApplication.playModeStateChanged += OnChangePlayModeState;
            }

            ScreenshotStore.CleanDirectories();

            await CallAttachedInitializeOnLaunchAutopilotAttributeMethods();

            var autopilot = new GameObject(nameof(Autopilot)).AddComponent<Autopilot>();
            Object.DontDestroyOnLoad(autopilot);
        }

        private static async UniTask CallAttachedInitializeOnLaunchAutopilotAttributeMethods()
        {
            foreach (var methodInfo in AppDomain.CurrentDomain.GetAssemblies()
                         .SelectMany(x => x.GetTypes())
                         .SelectMany(x => x.GetMethods())
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

        /// <summary>
        /// Stop autopilot on play mode exit event when run on Unity editor.
        /// Not called when invoked from play mode (not registered in event listener).
        /// </summary>
        /// <param name="playModeStateChange"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private static void OnChangePlayModeState(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange != PlayModeStateChange.EnteredEditMode)
            {
                return;
            }

            EditorApplication.playModeStateChanged -= OnChangePlayModeState;

            var state = AutopilotState.Instance;
            switch (state.launchFrom)
            {
                case LaunchType.EditorEditMode:
                    Debug.Log("Exit play mode");
                    state.Reset();
                    break;

                case LaunchType.Commandline:
                    // Exit Unity when returning from play mode to edit mode.
                    // Because it may freeze when exiting without going through edit mode.
                    var exitCode = (int)state.exitCode;
                    Debug.Log($"Exit Unity-editor by autopilot, exit code={exitCode}");
                    EditorApplication.Exit(exitCode);
                    break;

                default:
#pragma warning disable S3928
                    throw new ArgumentOutOfRangeException(nameof(state.launchFrom));
#pragma warning restore S3928
            }
        }
    }
}

#endif
