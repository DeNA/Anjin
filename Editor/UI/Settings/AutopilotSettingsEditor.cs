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
        private static readonly string s_description = L10n.Tr("Description");
        private static readonly string s_descriptionTooltip = L10n.Tr("Description about this setting instance");

        private static readonly string s_agentAssignmentHeader = L10n.Tr("Agent Assignment");
        private static readonly string s_sceneAgentMaps = L10n.Tr("Scene Agent Mapping");
        private static readonly string s_sceneAgentMapsTooltip = L10n.Tr("Scene to Agent assign mapping");
        private static readonly string s_fallbackAgent = L10n.Tr("Fallback Agent");
        private static readonly string s_fallbackAgentTooltip = L10n.Tr("Agent for when not assigned");
        private static readonly string s_observerAgent = L10n.Tr("Observer Agent");

        private static readonly string s_observerAgentTooltip =
            L10n.Tr("Parallel running agent using on every scene, for using observer (e.g., EmergencyExitAgent)");

        private static readonly string s_autopilotRunSettingsHeader = L10n.Tr("Autopilot Run Settings");
        private static readonly string s_lifespanSec = L10n.Tr("Lifespan Sec");

        private static readonly string s_lifespanSecTooltip =
            L10n.Tr("Autopilot running lifespan [sec]. When specified zero, so unlimited running");

        private static readonly string s_randomSeed = L10n.Tr("Random Seed");
        private static readonly string s_randomSeedTooltip = L10n.Tr("Random using the specified seed value");
        private static readonly string s_timeScale = L10n.Tr("Time Scale");
        private static readonly string s_timeScaleTooltip = L10n.Tr("Time.timeScale on running Autopilot");

        private static readonly string s_junitReportPath = L10n.Tr("JUnit Report Path");
        private static readonly string s_junitReportPathTooltip = L10n.Tr("JUnit report output path");

        private static readonly string s_logger = L10n.Tr("Logger");

        private static readonly string s_loggerTooltip =
            L10n.Tr("Logger used for this autopilot settings. If omitted, Debug.unityLogger will be used as default.");

        private static readonly string s_reporter = L10n.Tr("Reporter");

        private static readonly string s_reporterTooltip =
            L10n.Tr("Reporter that called when some errors occurred in target application");
        
        private static readonly string s_errorHandlingSettingsHeader = L10n.Tr("Error Handling Settings");
        private static readonly string s_handleException = L10n.Tr("Handle Exception");
        private static readonly string s_handleExceptionTooltip = L10n.Tr("Notify when Exception detected in log");
        private static readonly string s_handleError = L10n.Tr("Handle Error");
        private static readonly string s_handleErrorTooltip = L10n.Tr("Notify when Error detected in log");
        private static readonly string s_handleAssert = L10n.Tr("Handle Assert");
        private static readonly string s_handleAssertTooltip = L10n.Tr("Notify when Assert detected in log");
        private static readonly string s_handleWarning = L10n.Tr("Handle Warning");
        private static readonly string s_handleWarningTooltip = L10n.Tr("Notify when Warning detected in log");
        private static readonly string s_ignoreMessages = L10n.Tr("Ignore Messages");

        private static readonly string s_ignoreMessagesTooltip =
            L10n.Tr("Do not send notifications when log messages contain this string");

        private static readonly string s_runButton = L10n.Tr("Run");
        private static readonly string s_stopButton = L10n.Tr("Stop");
        private const float SpacerPixels = 10f;
        private const float SpacerPixelsUnderHeader = 4f;

        /// <inheritdoc/>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AutopilotSettings.description)),
                new GUIContent(s_description, s_descriptionTooltip));

            DrawHeader(s_agentAssignmentHeader);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AutopilotSettings.sceneAgentMaps)),
                new GUIContent(s_sceneAgentMaps, s_sceneAgentMapsTooltip), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AutopilotSettings.fallbackAgent)),
                new GUIContent(s_fallbackAgent, s_fallbackAgentTooltip));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AutopilotSettings.observerAgent)),
                new GUIContent(s_observerAgent, s_observerAgentTooltip));

            DrawHeader(s_autopilotRunSettingsHeader);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AutopilotSettings.lifespanSec)),
                new GUIContent(s_lifespanSec, s_lifespanSecTooltip));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AutopilotSettings.randomSeed)),
                new GUIContent(s_randomSeed, s_randomSeedTooltip));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AutopilotSettings.timeScale)),
                new GUIContent(s_timeScale, s_timeScaleTooltip));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AutopilotSettings.junitReportPath)),
                new GUIContent(s_junitReportPath, s_junitReportPathTooltip));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AutopilotSettings.loggerAsset)),
                new GUIContent(s_logger, s_loggerTooltip));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AutopilotSettings.reporter)),
                new GUIContent(s_reporter, s_reporterTooltip));

            DrawHeader(s_errorHandlingSettingsHeader);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AutopilotSettings.handleException)),
                new GUIContent(s_handleException, s_handleExceptionTooltip));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AutopilotSettings.handleError)),
                new GUIContent(s_handleError, s_handleErrorTooltip));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AutopilotSettings.handleAssert)),
                new GUIContent(s_handleAssert, s_handleAssertTooltip));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AutopilotSettings.handleWarning)),
                new GUIContent(s_handleWarning, s_handleWarningTooltip));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AutopilotSettings.ignoreMessages)),
                new GUIContent(s_ignoreMessages, s_ignoreMessagesTooltip), true);

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
            var autopilot = FindObjectOfType<Autopilot>();
            if (autopilot)
            {
                autopilot.TerminateAsync(ExitCode.Normally).Forget();
            }
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
                EditorApplication.isPlaying = true;
            }
        }
    }
}
