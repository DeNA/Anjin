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

        // @formatter:off
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

        private static readonly string s_mentionSubTeamIDs = L10n.Tr("Sub Team IDs to Mention");
        private static readonly string s_mentionSubTeamIDsTooltip = L10n.Tr("Sub team IDs to mention (comma separates)");
        private SerializedProperty _mentionSubTeamIDsProp;
        private GUIContent _mentionSubTeamIDsGUIContent;

        private static readonly string s_addHereInSlackMessage = L10n.Tr("Add @here Into Slack Message");
        private static readonly string s_addHereInSlackMessageTooltip = L10n.Tr("Whether adding @here into Slack messages or not");
        private SerializedProperty _addHereInSlackMessageProp;
        private GUIContent _addHereInSlackMessageGUIContent;

        private static readonly string s_messageBodyTemplateOnError = L10n.Tr("Message Body Template");
        private static readonly string s_messageBodyTemplateOnErrorTooltip = L10n.Tr("Message body template when posting an error terminated report. You can specify placeholders like a \"{message}\"; see the README for more information.");
        private SerializedProperty _messageBodyTemplateOnErrorProp;
        private GUIContent _messageBodyTemplateOnErrorGUIContent;

        private static readonly string s_colorOnError = L10n.Tr("Color");
        private static readonly string s_colorOnErrorTooltip = L10n.Tr("Attachment color when posting an error terminated report");
        private SerializedProperty _colorOnErrorProp;
        private GUIContent _colorOnErrorGUIContent;

        private static readonly string s_withScreenshotOnError = L10n.Tr("Screenshot");
        private static readonly string s_withScreenshotOnErrorTooltip = L10n.Tr("Take a screenshot when posting an error terminated report");
        private SerializedProperty _withScreenshotOnErrorProp;
        private GUIContent _withScreenshotOnErrorGUIContent;

        private static readonly string s_postOnNormally = L10n.Tr("Normally Terminated Report");
        private static readonly string s_postOnNormallyTooltip = L10n.Tr("Post a report if normally terminates.");
        private SerializedProperty _postOnNormallyProp;
        private GUIContent _postOnNormallyGUIContent;

        private static readonly string s_messageBodyTemplateOnNormally = L10n.Tr("Message Body Template");
        private static readonly string s_messageBodyTemplateOnNormallyTooltip = L10n.Tr("Message body template when posting a normally terminated report. You can specify placeholders like a \"{message}\"; see the README for more information.");
        private SerializedProperty _messageBodyTemplateOnNormallyProp;
        private GUIContent _messageBodyTemplateOnNormallyGUIContent;

        private static readonly string s_colorOnNormally = L10n.Tr("Color");
        private static readonly string s_colorOnNormallyTooltip = L10n.Tr("Attachment color when posting a normally terminated report");
        private SerializedProperty _colorOnNormallyProp;
        private GUIContent _colorOnNormallyGUIContent;

        private static readonly string s_withScreenshotOnNormally = L10n.Tr("Screenshot");
        private static readonly string s_withScreenshotOnNormallyTooltip = L10n.Tr("Take a screenshot when posting a normally terminated report");
        private SerializedProperty _withScreenshotOnNormallyProp;
        private GUIContent _withScreenshotOnNormallyGUIContent;
        // @formatter:on

        private void OnEnable()
        {
            Initialize();
        }

        private void Initialize()
        {
            // @formatter:off
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

            _messageBodyTemplateOnErrorProp = serializedObject.FindProperty(nameof(SlackReporter.messageBodyTemplateOnError));
            _messageBodyTemplateOnErrorGUIContent = new GUIContent(s_messageBodyTemplateOnError, s_messageBodyTemplateOnErrorTooltip);

            _colorOnErrorProp = serializedObject.FindProperty(nameof(SlackReporter.colorOnError));
            _colorOnErrorGUIContent = new GUIContent(s_colorOnError, s_colorOnErrorTooltip);

            _withScreenshotOnErrorProp = serializedObject.FindProperty(nameof(SlackReporter.withScreenshotOnError));
            _withScreenshotOnErrorGUIContent = new GUIContent(s_withScreenshotOnError, s_withScreenshotOnErrorTooltip);

            _postOnNormallyProp = serializedObject.FindProperty(nameof(SlackReporter.postOnNormally));
            _postOnNormallyGUIContent = new GUIContent(s_postOnNormally, s_postOnNormallyTooltip);

            _messageBodyTemplateOnNormallyProp = serializedObject.FindProperty(nameof(SlackReporter.messageBodyTemplateOnNormally));
            _messageBodyTemplateOnNormallyGUIContent = new GUIContent(s_messageBodyTemplateOnNormally, s_messageBodyTemplateOnNormallyTooltip);

            _colorOnNormallyProp = serializedObject.FindProperty(nameof(SlackReporter.colorOnNormally));
            _colorOnNormallyGUIContent = new GUIContent(s_colorOnNormally, s_colorOnNormallyTooltip);

            _withScreenshotOnNormallyProp = serializedObject.FindProperty(nameof(SlackReporter.withScreenshotOnNormally));
            _withScreenshotOnNormallyGUIContent = new GUIContent(s_withScreenshotOnNormally, s_withScreenshotOnNormallyTooltip);
            // @formatter:on

            serializedObject.UpdateIfRequiredOrScript();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_descriptionProp, _descriptionGUIContent);
            GUILayout.Space(SpacerPixels);

            EditorGUILayout.PropertyField(_slackTokenProp, _slackTokenGUIContent);
            EditorGUILayout.PropertyField(_slackChannelsProp, _slackChannelsGUIContent);

            GUILayout.Space(SpacerPixels);
            EditorGUILayout.PropertyField(_mentionSubTeamIDsProp, _mentionSubTeamIDsGUIContent);
            EditorGUILayout.PropertyField(_addHereInSlackMessageProp, _addHereInSlackMessageGUIContent);

            GUILayout.Space(SpacerPixels);
            EditorGUILayout.PropertyField(_messageBodyTemplateOnErrorProp, _messageBodyTemplateOnErrorGUIContent);
            EditorGUILayout.PropertyField(_colorOnErrorProp, _colorOnErrorGUIContent);
            EditorGUILayout.PropertyField(_withScreenshotOnErrorProp, _withScreenshotOnErrorGUIContent);

            GUILayout.Space(SpacerPixels);
            EditorGUILayout.PropertyField(_postOnNormallyProp, _postOnNormallyGUIContent);

            EditorGUI.BeginDisabledGroup(!_postOnNormallyProp.boolValue);
            EditorGUILayout.PropertyField(_messageBodyTemplateOnNormallyProp, _messageBodyTemplateOnNormallyGUIContent);
            EditorGUILayout.PropertyField(_colorOnNormallyProp, _colorOnNormallyGUIContent);
            EditorGUILayout.PropertyField(_withScreenshotOnNormallyProp, _withScreenshotOnNormallyGUIContent);
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
