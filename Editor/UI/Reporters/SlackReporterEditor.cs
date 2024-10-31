// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using DeNA.Anjin.Reporters;
using UnityEditor;
using UnityEngine;

namespace DeNA.Anjin.Editor.UI.Reporters
{
    /// <summary>
    /// Editor for <c cref="SlackReporter" />
    /// </summary>
    [CustomEditor(typeof(SlackReporter))]
    public class SlackReporterEditor : UnityEditor.Editor
    {
        private const float SpacerPixels = 10f;
        private const float SpacerPixelsUnderHeader = 4f;

        private static readonly string s_description = L10n.Tr("Description");
        private static readonly string s_descriptionTooltip = L10n.Tr("Description about this Reporter instance");
        private SerializedProperty _descriptionProp;
        private GUIContent _descriptionGUIContent;

        private static readonly string s_slackToken = L10n.Tr("Slack Token");
        private static readonly string s_slackTokenTooltip = L10n.Tr("Slack API token");
        private SerializedProperty _slackTokenProp;
        private GUIContent _slackTokenGUIContent;

        private static readonly string s_slackChannels = L10n.Tr("Slack Channels");
        private static readonly string s_slackChannelsTooltip = L10n.Tr("Slack channels to send notification");
        private SerializedProperty _slackChannelsProp;
        private GUIContent _slackChannelsGUIContent;

        private static readonly string s_slackMentionSettingsHeader = L10n.Tr("Slack Mention Settings");

        private static readonly string s_mentionSubTeamIDs = L10n.Tr("Sub Team IDs to Mention");
        private static readonly string s_mentionSubTeamIDsTooltip = L10n.Tr("Sub team IDs to mention (comma separates)");
        private SerializedProperty _mentionSubTeamIDsProp;
        private GUIContent _mentionSubTeamIDsGUIContent;

        private static readonly string s_addHereInSlackMessage = L10n.Tr("Add @here Into Slack Message");
        private static readonly string s_addHereInSlackMessageTooltip = L10n.Tr("Whether adding @here into Slack messages or not");
        private SerializedProperty _addHereInSlackMessageProp;
        private GUIContent _addHereInSlackMessageGUIContent;

        private static readonly string s_withScreenshot = L10n.Tr("Take screenshot");
        private static readonly string s_withScreenshotTooltip = L10n.Tr("Take screenshot when posting report");
        private SerializedProperty _withScreenshotProp;
        private GUIContent _withScreenshotGUIContent;


        private void OnEnable()
        {
            Initialize();
        }


        private void Initialize()
        {
            _descriptionProp = serializedObject.FindProperty(nameof(SlackReporter.description));
            _descriptionGUIContent = new GUIContent(s_description, s_descriptionTooltip);

            _slackTokenProp = serializedObject.FindProperty(nameof(SlackReporter.slackToken));
            _slackTokenGUIContent = new GUIContent(s_slackToken, s_slackTokenTooltip);

            _slackChannelsProp = serializedObject.FindProperty(nameof(SlackReporter.slackChannels));
            _slackChannelsGUIContent = new GUIContent(s_slackChannels, s_slackChannelsTooltip);

            _mentionSubTeamIDsProp = serializedObject.FindProperty(nameof(SlackReporter.mentionSubTeamIDs));
            _mentionSubTeamIDsGUIContent = new GUIContent(s_mentionSubTeamIDs, s_mentionSubTeamIDsTooltip);

            _addHereInSlackMessageProp = serializedObject.FindProperty(nameof(SlackReporter.addHereInSlackMessage));
            _addHereInSlackMessageGUIContent = new GUIContent(s_addHereInSlackMessage, s_addHereInSlackMessageTooltip);

            _withScreenshotProp = serializedObject.FindProperty(nameof(SlackReporter.withScreenshot));
            _withScreenshotGUIContent = new GUIContent(s_withScreenshot, s_withScreenshotTooltip);
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_descriptionProp, _descriptionGUIContent);
            GUILayout.Space(SpacerPixels);

            EditorGUILayout.PropertyField(_slackTokenProp, _slackTokenGUIContent);
            EditorGUILayout.PropertyField(_slackChannelsProp, _slackChannelsGUIContent);

            GUILayout.Space(SpacerPixels);
            GUILayout.Label(s_slackMentionSettingsHeader);
            GUILayout.Space(SpacerPixelsUnderHeader);

            EditorGUILayout.PropertyField(_mentionSubTeamIDsProp, _mentionSubTeamIDsGUIContent);
            EditorGUILayout.PropertyField(_addHereInSlackMessageProp, _addHereInSlackMessageGUIContent);

            GUILayout.Space(SpacerPixels);
            EditorGUILayout.PropertyField(_withScreenshotProp, _withScreenshotGUIContent);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
