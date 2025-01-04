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
        private const float SpacerSectionPixels = 18f;
        private const float SpacerUnderHeaderPixels = 8f;

        // @formatter:off
        private static readonly string s_description = L10n.Tr("Description");
        private static readonly string s_descriptionTooltip = L10n.Tr("Description about this Reporter instance.");
        private SerializedProperty _descriptionProp;
        private GUIContent _descriptionGUIContent;

        private static readonly string s_slackToken = L10n.Tr("Slack Token");
        private static readonly string s_slackTokenTooltip = L10n.Tr("Slack Bot OAuth token. If omitted, it will not be sent.");
        private SerializedProperty _slackTokenProp;
        private GUIContent _slackTokenGUIContent;

        private static readonly string s_slackChannels = L10n.Tr("Slack Channels");
        private static readonly string s_slackChannelsTooltip = L10n.Tr("Comma-separated Slack channel's IDs (e.g., \"C123456\") to post. If omitted, it will not be sent.");
        private SerializedProperty _slackChannelsProp;
        private GUIContent _slackChannelsGUIContent;

        private static readonly string s_errorSettingsHeader = L10n.Tr("Error Report Settings");

        private static readonly string s_mentionSubTeamIDs = L10n.Tr("Mention User Group IDs");
        private static readonly string s_mentionSubTeamIDsTooltip = L10n.Tr("Comma-separated user group IDs to mention when posting error reports.");
        private SerializedProperty _mentionSubTeamIDsProp;
        private GUIContent _mentionSubTeamIDsGUIContent;

        private static readonly string s_addHereInSlackMessage = L10n.Tr("Add @here");
        private static readonly string s_addHereInSlackMessageTooltip = L10n.Tr("Add @here to the post when posting error reports.");
        private SerializedProperty _addHereInSlackMessageProp;
        private GUIContent _addHereInSlackMessageGUIContent;

        private static readonly string s_leadTextOnError = L10n.Tr("Lead Text");
        private static readonly string s_leadTextOnErrorTooltip = L10n.Tr("Lead text for error reports. It is used in OS notifications. You can specify placeholders like a \"{message}\"; see the README for more information.");
        private SerializedProperty _leadTextOnErrorProp;
        private GUIContent _leadTextOnErrorGUIContent;

        private static readonly string s_messageBodyTemplateOnError = L10n.Tr("Message");
        private static readonly string s_messageBodyTemplateOnErrorTooltip = L10n.Tr("Message body template for error reports. You can specify placeholders like a \"{message}\"; see the README for more information.");
        private SerializedProperty _messageBodyTemplateOnErrorProp;
        private GUIContent _messageBodyTemplateOnErrorGUIContent;

        private static readonly string s_colorOnError = L10n.Tr("Color");
        private static readonly string s_colorOnErrorTooltip = L10n.Tr("Attachments color for error reports.");
        private SerializedProperty _colorOnErrorProp;
        private GUIContent _colorOnErrorGUIContent;

        private static readonly string s_withScreenshotOnError = L10n.Tr("Screenshot");
        private static readonly string s_withScreenshotOnErrorTooltip = L10n.Tr("Take a screenshot for error reports.");
        private SerializedProperty _withScreenshotOnErrorProp;
        private GUIContent _withScreenshotOnErrorGUIContent;

        private static readonly string s_normallyHeader = L10n.Tr("Completion Report Settings");

        private static readonly string s_postOnNormally = L10n.Tr("Post on Completion");
        private static readonly string s_postOnNormallyTooltip = L10n.Tr("Also post a report if completed autopilot normally.");
        private SerializedProperty _postOnNormallyProp;
        private GUIContent _postOnNormallyGUIContent;

        private static readonly string s_mentionSubTeamIDsOnNormally = L10n.Tr("Mention User Group IDs");
        private static readonly string s_mentionSubTeamIDsOnNormallyTooltip = L10n.Tr("Comma-separated user group IDs to mention when posting completion reports.");
        private SerializedProperty _mentionSubTeamIDsOnNormallyProp;
        private GUIContent _mentionSubTeamIDsOnNormallyGUIContent;

        private static readonly string s_addHereInSlackMessageOnNormally = L10n.Tr("Add @here");
        private static readonly string s_addHereInSlackMessageOnNormallyTooltip = L10n.Tr("Add @here to the post when posting completion reports.");
        private SerializedProperty _addHereInSlackMessageOnNormallyProp;
        private GUIContent _addHereInSlackMessageOnNormallyGUIContent;

        private static readonly string s_leadTextOnNormally = L10n.Tr("Lead Text");
        private static readonly string s_leadTextOnNormallyTooltip = L10n.Tr("Lead text for completion reports. It is used in OS notifications. You can specify placeholders like a \"{message}\"; see the README for more information.");
        private SerializedProperty _leadTextOnNormallyProp;
        private GUIContent _leadTextOnNormallyGUIContent;

        private static readonly string s_messageBodyTemplateOnNormally = L10n.Tr("Message");
        private static readonly string s_messageBodyTemplateOnNormallyTooltip = L10n.Tr("Message body template for completion reports. You can specify placeholders like a \"{message}\"; see the README for more information.");
        private SerializedProperty _messageBodyTemplateOnNormallyProp;
        private GUIContent _messageBodyTemplateOnNormallyGUIContent;

        private static readonly string s_colorOnNormally = L10n.Tr("Color");
        private static readonly string s_colorOnNormallyTooltip = L10n.Tr("Attachments color for completion reports.");
        private SerializedProperty _colorOnNormallyProp;
        private GUIContent _colorOnNormallyGUIContent;

        private static readonly string s_withScreenshotOnNormally = L10n.Tr("Screenshot");
        private static readonly string s_withScreenshotOnNormallyTooltip = L10n.Tr("Take a screenshot for completion reports.");
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

            _leadTextOnErrorProp = serializedObject.FindProperty(nameof(SlackReporter.leadTextOnError));
            _leadTextOnErrorGUIContent = new GUIContent(s_leadTextOnError, s_leadTextOnErrorTooltip);

            _messageBodyTemplateOnErrorProp = serializedObject.FindProperty(nameof(SlackReporter.messageBodyTemplateOnError));
            _messageBodyTemplateOnErrorGUIContent = new GUIContent(s_messageBodyTemplateOnError, s_messageBodyTemplateOnErrorTooltip);

            _colorOnErrorProp = serializedObject.FindProperty(nameof(SlackReporter.colorOnError));
            _colorOnErrorGUIContent = new GUIContent(s_colorOnError, s_colorOnErrorTooltip);

            _withScreenshotOnErrorProp = serializedObject.FindProperty(nameof(SlackReporter.withScreenshotOnError));
            _withScreenshotOnErrorGUIContent = new GUIContent(s_withScreenshotOnError, s_withScreenshotOnErrorTooltip);

            _postOnNormallyProp = serializedObject.FindProperty(nameof(SlackReporter.postOnNormally));
            _postOnNormallyGUIContent = new GUIContent(s_postOnNormally, s_postOnNormallyTooltip);

            _mentionSubTeamIDsOnNormallyProp = serializedObject.FindProperty(nameof(SlackReporter.mentionSubTeamIDsOnNormally));
            _mentionSubTeamIDsOnNormallyGUIContent = new GUIContent(s_mentionSubTeamIDsOnNormally, s_mentionSubTeamIDsOnNormallyTooltip);

            _addHereInSlackMessageOnNormallyProp = serializedObject.FindProperty(nameof(SlackReporter.addHereInSlackMessageOnNormally));
            _addHereInSlackMessageOnNormallyGUIContent = new GUIContent(s_addHereInSlackMessageOnNormally, s_addHereInSlackMessageOnNormallyTooltip);

            _leadTextOnNormallyProp = serializedObject.FindProperty(nameof(SlackReporter.leadTextOnNormally));
            _leadTextOnNormallyGUIContent = new GUIContent(s_leadTextOnNormally, s_leadTextOnNormallyTooltip);

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
            GUILayout.Space(SpacerSectionPixels);

            GUILayout.Label(s_errorSettingsHeader);
            GUILayout.Space(SpacerUnderHeaderPixels);
            EditorGUILayout.PropertyField(_mentionSubTeamIDsProp, _mentionSubTeamIDsGUIContent);
            EditorGUILayout.PropertyField(_addHereInSlackMessageProp, _addHereInSlackMessageGUIContent);
            EditorGUILayout.PropertyField(_leadTextOnErrorProp, _leadTextOnErrorGUIContent);
            EditorGUILayout.PropertyField(_messageBodyTemplateOnErrorProp, _messageBodyTemplateOnErrorGUIContent);
            EditorGUILayout.PropertyField(_colorOnErrorProp, _colorOnErrorGUIContent);
            EditorGUILayout.PropertyField(_withScreenshotOnErrorProp, _withScreenshotOnErrorGUIContent);
            GUILayout.Space(SpacerSectionPixels);

            GUILayout.Label(s_normallyHeader);
            GUILayout.Space(SpacerUnderHeaderPixels);
            EditorGUILayout.PropertyField(_postOnNormallyProp, _postOnNormallyGUIContent);

            EditorGUI.BeginDisabledGroup(!_postOnNormallyProp.boolValue);
            EditorGUILayout.PropertyField(_mentionSubTeamIDsOnNormallyProp, _mentionSubTeamIDsOnNormallyGUIContent);
            EditorGUILayout.PropertyField(_addHereInSlackMessageOnNormallyProp, _addHereInSlackMessageOnNormallyGUIContent);
            EditorGUILayout.PropertyField(_leadTextOnNormallyProp, _leadTextOnNormallyGUIContent);
            EditorGUILayout.PropertyField(_messageBodyTemplateOnNormallyProp, _messageBodyTemplateOnNormallyGUIContent);
            EditorGUILayout.PropertyField(_colorOnNormallyProp, _colorOnNormallyGUIContent);
            EditorGUILayout.PropertyField(_withScreenshotOnNormallyProp, _withScreenshotOnNormallyGUIContent);
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
