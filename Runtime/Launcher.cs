// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Attributes;
using DeNA.Anjin.Settings;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using AssertionException = NUnit.Framework.AssertionException;
#endif
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
        /// Launch autopilot from Play Mode tests or runtime (e.g., debug menu).
        /// </summary>
        /// <param name="settings">Autopilot settings</param>
        /// <param name="token">Task cancellation token</param>
        public static async UniTask LaunchAutopilotAsync(AutopilotSettings settings, CancellationToken token = default)
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

            await UniTask.WaitUntil(() => !state.IsRunning, cancellationToken: token);

#if UNITY_INCLUDE_TESTS
            // Launch from Play Mode tests
            if (TestContext.CurrentContext != null && state.exitCode != ExitCode.Normally)
            {
                throw new AssertionException($"  Autopilot run failed with exit code \"{state.exitCode}\".");
            }
#endif
        }

        /// <summary>
        /// Launch autopilot from Play Mode tests or runtime (e.g., debug menu).
        /// </summary>
        /// <param name="settingsPath">Asset file path for autopilot settings. When running the player, it reads from <c>Resources</c></param>
        /// <param name="token">Task cancellation token</param>
        public static async UniTask LaunchAutopilotAsync(string settingsPath, CancellationToken token = default)
        {
#if UNITY_EDITOR
            var settings = AssetDatabase.LoadAssetAtPath<AutopilotSettings>(settingsPath);
#else
            var settings = Resources.Load<AutopilotSettings>(settingsPath);
#endif
            if (settings == null)
            {
                throw new ArgumentException($"Autopilot settings not found: {settingsPath}");
            }

            await LaunchAutopilotAsync(settings, token);
        }

        /// <summary>
        /// Launch autopilot on player build with command line arguments.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LaunchAutopilotOnPlayerFromCommandline()
        {
            if (Application.isEditor)
            {
                return;
            }

            var args = new Arguments();
            if (!args.LaunchAutopilotSettings.IsCaptured())
            {
                return;
            }

            var settings = Resources.Load<AutopilotSettings>(args.LaunchAutopilotSettings.Value());
            var state = AutopilotState.Instance;
            if (state.IsRunning)
            {
                throw new InvalidOperationException("Autopilot is already running");
            }

            state.launchFrom = LaunchType.Commandline;
            state.settings = settings;
        }

        /// <summary>
        /// Launch autopilot (internal).
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

            try
            {
                await CallInitializeOnLaunchAutopilotMethods();
            }
            catch (Exception e)
            {
                Debug.LogException(e);

                var logger = Debug.unityLogger; // Note: Logger is not initialized yet.
                const ExitCode ExitCode = ExitCode.AutopilotLaunchingFailed;
                const string Caller = "Launcher";
                Debug.Log("Cancel launching Autopilot");
                TeardownLaunchAutopilotAsync(state, logger, ExitCode, Caller).Forget();
                return;
            }

            var autopilot = new GameObject(nameof(Autopilot)).AddComponent<Autopilot>();
            Object.DontDestroyOnLoad(autopilot);
        }

        private static async UniTask CallInitializeOnLaunchAutopilotMethods()
        {
            var orderedMethodMap = GetOrderedInitializeOnLaunchAutopilotMethodsMap();
            // key: order, value: methods attaching InitializeOnLaunchAutopilotAttribute.

            var tasks = new List<Task>();
            var uniTasks = new List<UniTask>();

            foreach (var order in orderedMethodMap.Keys.OrderBy(x => x))
            {
                tasks.Clear();
                uniTasks.Clear();

                foreach (var method in orderedMethodMap[order])
                {
                    try
                    {
                        switch (method.ReturnType.Name)
                        {
                            case nameof(Task):
                                tasks.Add((Task)method.Invoke(null, null));
                                break;
                            case nameof(UniTask):
                                uniTasks.Add((UniTask)method.Invoke(null, null));
                                break;
                            default:
                                method.Invoke(null, null); // static method only
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        Debug.LogError($"Invoke method failed: {method.Name}"); // Note: Logger is not initialized yet.
                        throw;
                    }
                }

                await Task.WhenAll(tasks);
                await UniTask.WhenAll(uniTasks);
            }
        }

        private static Dictionary<int, List<MethodInfo>> GetOrderedInitializeOnLaunchAutopilotMethodsMap()
        {
            var orderedMethodMap = new Dictionary<int, List<MethodInfo>>();
            foreach (var (order, method) in GetInitializeOnLaunchAutopilotMethods())
            {
                if (!orderedMethodMap.TryGetValue(order, out var methods))
                {
                    methods = new List<MethodInfo>();
                    orderedMethodMap[order] = methods;
                }

                methods.Add(method);
            }

            return orderedMethodMap;
        }

        private static IEnumerable<(int, MethodInfo)> GetInitializeOnLaunchAutopilotMethods()
        {
#if UNITY_EDITOR && UNITY_2021_3_OR_NEWER
            return TypeCache.GetMethodsWithAttribute<InitializeOnLaunchAutopilotAttribute>()
                .Select(x => (x.GetCustomAttribute<InitializeOnLaunchAutopilotAttribute>().CallbackOrder, x))
                .OrderBy(x => x.Item1)
                .Select(x => (x.Item1, x.Item2));
#else
            const BindingFlags MethodBindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (var method in AppDomain.CurrentDomain.GetAssemblies()
                         .SelectMany(x => x.GetTypes())
                         .SelectMany(x => x.GetMethods(MethodBindingFlags)))
            {
                var attribute = method.GetCustomAttributes<InitializeOnLaunchAutopilotAttribute>(false)
                    .FirstOrDefault();
                if (attribute != null)
                {
                    yield return (attribute.CallbackOrder, method);
                }
            }
#endif
        }

        internal static async UniTaskVoid TeardownLaunchAutopilotAsync(AutopilotState state, ILogger logger,
            ExitCode exitCode, string caller)
        {
            state.settings = null;
            state.exitCode = exitCode;
#if UNITY_EDITOR && UNITY_2020_3_OR_NEWER
            EditorUtility.SetDirty(state);
            AssetDatabase.SaveAssetIfDirty(state); // Note: Sync with virtual players of MPPM package
#endif

            if (state.launchFrom == LaunchType.PlayMode) // Note: Editor play mode, Play mode tests, and Player build
            {
                return; // Only terminate autopilot run if starting from play mode.
            }

            if (Application.isEditor)
            {
                // Terminate when launch from edit mode (including launch from commandline)
                logger.Log($"Stop playing by {caller}");

                // XXX: Avoid a problem that Editor stay playing despite isPlaying get assigned false.
                // SEE: https://github.com/DeNA/Anjin/issues/20
                await UniTask.NextFrame();
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
                // Note: If launched from the command line, `DeNA.Anjin.Autopilot.OnExitPlayModeToTerminateEditor()` will be called, and the Unity editor will be terminated.
#endif
            }
            else
            {
                // Player build launch from commandline
                logger.Log($"Exit Unity-player by {caller}, exit code: {(int)exitCode}");
                Application.Quit((int)exitCode);
            }
        }
    }
}
