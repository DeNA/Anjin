// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using Cysharp.Threading.Tasks;
using DeNA.Anjin.Settings;
using UnityEditor;
using UnityEngine;

namespace DeNA.Anjin.Editor.UI.Settings
{
    /// <summary>
    /// Autopilot Settings inspector UI
    /// </summary>
    [CustomEditor(typeof(AutopilotSettings))]
    public class AutopilotSettingsEditor : UnityEditor.Editor
    {
        // @formatter:off
        private static readonly string s_description = L10n.Tr("Description");
        private static readonly string s_descriptionTooltip = L10n.Tr("Description about this setting instance");

        private static readonly string s_name = L10n.Tr("Name");
        private static readonly string s_nameTooltip = L10n.Tr("Custom name of this setting used by Reporter. If omitted, the asset file name is used.");

        private static readonly string s_agentAssignmentHeader = L10n.Tr("Agent Assignment");
        private static readonly string s_sceneAgentMaps = L10n.Tr("Scene Agent Mapping");
        private static readonly string s_sceneAgentMapsTooltip = L10n.Tr("Scene to Agent assign mapping");
        private static readonly string s_fallbackAgent = L10n.Tr("Fallback Agent");
        private static readonly string s_fallbackAgentTooltip = L10n.Tr("Agent for when not assigned");

        private static readonly string s_sceneCrossingAgents = L10n.Tr("Scene Crossing Agents");
        private static readonly string s_sceneCrossingAgentsTooltip = L10n.Tr("Agents running by scene crossing. The specified agents will have the same lifespan as Autopilot (i.e., use DontDestroyOnLoad) for specifying, e.g., ErrorHandlerAgent and UGUIEmergencyExitAgent.");

        private static readonly string s_autopilotRunSettingsHeader = L10n.Tr("Autopilot Run Settings");
        private static readonly string s_lifespanSec = L10n.Tr("Lifespan Sec");
        private static readonly string s_lifespanSecTooltip = L10n.Tr("Autopilot running lifespan [sec]. When specified zero, so unlimited running");

        private static readonly string s_randomSeed = L10n.Tr("Random Seed");
        private static readonly string s_randomSeedTooltip = L10n.Tr("Random using the specified seed value");
        private static readonly string s_timeScale = L10n.Tr("Time Scale");
        private static readonly string s_timeScaleTooltip = L10n.Tr("Time.timeScale on running Autopilot");

        private static readonly string s_loggers = L10n.Tr("Loggers");
        private static readonly string s_loggersTooltip = L10n.Tr("List of Loggers used for this autopilot settings. If omitted, Debug.unityLogger will be used as default.");

        private static readonly string s_reporters = L10n.Tr("Reporters");
        private static readonly string s_reportersTooltip = L10n.Tr("List of Reporters to be called on Autopilot terminate.");

        private static readonly string s_runButton = L10n.Tr("Run");
        private static readonly string s_stopButton = L10n.Tr("Stop");
        private const float SpacerPixels = 10f;
        private const float SpacerPixelsUnderHeader = 4f;
        // @formatter:on

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AutopilotSettings.description)),
                new GUIContent(s_description, s_descriptionTooltip));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AutopilotSettings.name)),
                new GUIContent(s_name, s_nameTooltip));

            DrawHeader(s_agentAssignmentHeader);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AutopilotSettings.sceneAgentMaps)),
                new GUIContent(s_sceneAgentMaps, s_sceneAgentMapsTooltip), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AutopilotSettings.fallbackAgent)),
                new GUIContent(s_fallbackAgent, s_fallbackAgentTooltip));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AutopilotSettings.sceneCrossingAgents)),
                new GUIContent(s_sceneCrossingAgents, s_sceneCrossingAgentsTooltip));

            DrawHeader(s_autopilotRunSettingsHeader);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AutopilotSettings.lifespanSec)),
                new GUIContent(s_lifespanSec, s_lifespanSecTooltip));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AutopilotSettings.randomSeed)),
                new GUIContent(s_randomSeed, s_randomSeedTooltip));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AutopilotSettings.timeScale)),
                new GUIContent(s_timeScale, s_timeScaleTooltip));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AutopilotSettings.loggerAssets)),
                new GUIContent(s_loggers, s_loggersTooltip));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AutopilotSettings.reporters)),
                new GUIContent(s_reporters, s_reportersTooltip));

            serializedObject.ApplyModifiedProperties();

            GUILayout.Space(SpacerPixels);

            var state = AutopilotState.Instance;
            if (state.IsRunning)
            {
                if (GUILayout.Button(s_stopButton))
                {
                    Stop();
                }
            }
            else
            {
                if (GUILayout.Button(s_runButton))
                {
                    Launch();
                }
            }
        }

        private static void DrawHeader(string label)
        {
            GUILayout.Space(SpacerPixels);
            GUILayout.Label(label);
            GUILayout.Space(SpacerPixelsUnderHeader);
        }

        // ReSharper disable once MemberCanBeMadeStatic.Global
        internal void Stop()
        {
            Autopilot.Instance.TerminateAsync(ExitCode.Normally, reporting: false).Forget();
        }

        internal void Launch()
        {
            var state = AutopilotState.Instance;
            state.settings = (AutopilotSettings)target;

            if (EditorApplication.isPlaying)
            {
                state.launchFrom = LaunchType.PlayMode;
                Launcher.LaunchAutopilot().Forget();
            }
            else
            {
                state.launchFrom = LaunchType.EditMode;
                AssetDatabase.SaveAssetIfDirty(state); // Note: Sync with virtual players of MPPM package
                EditorApplication.isPlaying = true;
            }
        }
    }
}
