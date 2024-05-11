// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Settings;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

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
        private static readonly string s_loggerTooltip = L10n.Tr("Logger used for this autopilot settings. If omitted, Debug.unityLogger will be used as default.");

        private static readonly string s_reporter = L10n.Tr("Reporter");
        private static readonly string s_reporterTooltip = L10n.Tr("Reporter that called when some errors occurred in target application");
        
        private static readonly string s_slackToken = L10n.Tr("Slack Token");
        private static readonly string s_slackTokenTooltip = L10n.Tr("Slack API token");
        private static readonly string s_slackChannels = L10n.Tr("Slack Channels");
        private static readonly string s_slackChannelsTooltip = L10n.Tr("Slack channels to send notification");

        private static readonly string s_slackMentionSettingsHeader = L10n.Tr("Slack Mention Settings");
        private static readonly string s_mentionSubTeamIDs = L10n.Tr("Sub Team IDs to Mention");
        private static readonly string s_mentionSubTeamIDsTooltip = L10n.Tr("Sub team IDs to mention (comma separates)");
        private static readonly string s_addHereInSlackMessage = L10n.Tr("Add @here Into Slack Message");
        private static readonly string s_addHereInSlackMessageTooltip = L10n.Tr("Whether adding @here into Slack messages or not");

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

        private static readonly string s_obsoletedSlackParamsHelpBox =
            L10n.Tr("Slack settings will be moved to SlackReporter");

        private static readonly string s_ignoreMessagesTooltip =
            L10n.Tr("Do not send notifications when log messages contain this string");

        private static readonly string s_runButton = L10n.Tr("Run");
        private static readonly string s_stopButton = L10n.Tr("Stop");
        private const float SpacerPixels = 10f;
        private const float SpacerPixelsUnderHeader = 4f;

        private AutopilotSettings _settings;

        private void OnEnable()
        {
            _settings = target as AutopilotSettings;
            Assert.IsNotNull(_settings);
        }

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
            EditorGUILayout.HelpBox(s_obsoletedSlackParamsHelpBox, MessageType.Warning);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AutopilotSettings.slackToken)),
                new GUIContent(s_slackToken, s_slackTokenTooltip));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AutopilotSettings.slackChannels)),
                new GUIContent(s_slackChannels, s_slackChannelsTooltip));

            DrawHeader(s_slackMentionSettingsHeader);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(AutopilotSettings.mentionSubTeamIDs)),
                new GUIContent(s_mentionSubTeamIDs, s_mentionSubTeamIDsTooltip));
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty(nameof(AutopilotSettings.addHereInSlackMessage)),
                new GUIContent(s_addHereInSlackMessage, s_addHereInSlackMessageTooltip));

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
                    Stop().Forget();
                }
            }
            else
            {
                if (GUILayout.Button(s_runButton))
                {
                    Launch(state);
                }
            }
        }

        private static void DrawHeader(string label)
        {
            GUILayout.Space(SpacerPixels);
            GUILayout.Label(label);
            GUILayout.Space(SpacerPixelsUnderHeader);
        }

        [SuppressMessage("ApiDesign", "RS0030")]
        internal async UniTask Stop()
        {
            var autopilot = FindObjectOfType<Autopilot>();
            await autopilot.TerminateAsync(ExitCode.Normally);
        }

        internal void Launch(AutopilotState state)
        {
            state.settings = _settings;
            if (EditorApplication.isPlaying)
            {
                state.launchFrom = LaunchType.EditorPlayMode;
                Launcher.Run();
            }
            else
            {
                state.launchFrom = LaunchType.EditorEditMode;
                EditorApplication.isPlaying = true;
            }
        }
    }
}
