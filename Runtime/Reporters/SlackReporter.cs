// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Threading;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Reporters.Slack;
using DeNA.Anjin.Settings;
using UnityEngine;

namespace DeNA.Anjin.Reporters
{
    /// <summary>
    /// Post report to Slack
    /// </summary>
    [CreateAssetMenu(fileName = "New SlackReporter", menuName = "Anjin/Slack Reporter", order = 91)]
    public class SlackReporter : AbstractReporter
    {
        /// <summary>
        /// Slack Bot OAuth token.
        /// </summary>
        public string slackToken;

        /// <summary>
        /// Comma-separated Slack channel name (e.g., "#test") or ID (e.g., "C123456") to post. If omitted, it will not be sent.
        /// </summary>
        public string slackChannels;

        /// <summary>
        /// Comma-separated user group IDs to mention when posting error reports.
        /// </summary>
        public string mentionSubTeamIDs;

        /// <summary>
        /// Add @here to the post when posting error reports.
        /// </summary>
        public bool addHereInSlackMessage;

        /// <summary>
        /// Lead text for error reports. It is used in OS notifications.
        /// </summary>
        [Multiline]
        public string leadTextOnError = "{settings} occurred an error.";

        /// <summary>
        /// Message body template for error reports.
        /// </summary>
        [Multiline]
        public string messageBodyTemplateOnError = "{message}";

        /// <summary>
        /// Attachments color for error reports.
        /// </summary>
        public Color colorOnError = new Color(0.64f, 0.01f, 0f);

        /// <summary>
        /// Take a screenshot for error reports.
        /// </summary>
        public bool withScreenshotOnError = true;

        /// <summary>
        /// Also post a report if completed autopilot normally.
        /// </summary>
        public bool postOnNormally;

        /// <summary>
        /// Comma-separated user group IDs to mention when posting completion reports.
        /// </summary>
        public string mentionSubTeamIDsOnNormally;

        /// <summary>
        /// Add @here to the post when posting completion reports.
        /// </summary>
        public bool addHereInSlackMessageOnNormally;

        /// <summary>
        /// Lead text for completion reports.
        /// </summary>
        [Multiline]
        public string leadTextOnNormally = "{settings} completed normally.";

        /// <summary>
        /// Message body template for completion reports.
        /// </summary>
        [Multiline]
        public string messageBodyTemplateOnNormally = "{message}";

        /// <summary>
        /// Attachments color for completion reports.
        /// </summary>
        public Color colorOnNormally = new Color(0.24f, 0.65f, 0.34f);

        /// <summary>
        /// Take a screenshot for completion reports.
        /// </summary>
        public bool withScreenshotOnNormally;

        internal ISlackMessageSender _sender = new SlackMessageSender(new SlackAPI());

        private static ILogger Logger
        {
            get
            {
                var settings = AutopilotState.Instance.settings;
                if (settings != null)
                {
                    return settings.LoggerAsset.Logger;
                }

                Debug.LogWarning("Autopilot is not running"); // impossible to reach here
                return null;
            }
        }

        /// <inheritdoc />
        public override async UniTask PostReportAsync(
            string message,
            string stackTrace,
            ExitCode exitCode,
            CancellationToken cancellationToken = default
        )
        {
            if (exitCode == ExitCode.Normally && !postOnNormally)
            {
                return;
            }

            var mention = (exitCode == ExitCode.Normally)
                ? mentionSubTeamIDsOnNormally
                : mentionSubTeamIDs;
            var here = (exitCode == ExitCode.Normally)
                ? addHereInSlackMessageOnNormally
                : addHereInSlackMessage;
            var lead = MessageBuilder.BuildWithTemplate((exitCode == ExitCode.Normally)
                ? leadTextOnNormally
                : leadTextOnError);
            var messageBody = MessageBuilder.BuildWithTemplate((exitCode == ExitCode.Normally)
                    ? messageBodyTemplateOnNormally
                    : messageBodyTemplateOnError,
                message);
            var color = (exitCode == ExitCode.Normally) ? colorOnNormally : colorOnError;
            var withScreenshot = (exitCode == ExitCode.Normally) ? withScreenshotOnNormally : withScreenshotOnError;

            OverwriteByCommandlineArguments();
            if (string.IsNullOrEmpty(slackToken) || string.IsNullOrEmpty(slackChannels))
            {
                Logger?.Log(LogType.Warning, "Slack token or channels is empty");
                return;
            }

            // NOTE: In _sender.send, switch the execution thread to the main thread, so UniTask.WhenAll is meaningless.
            foreach (var slackChannel in slackChannels.Split(','))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                await PostReportAsync(slackChannel.Trim(), mention, here, lead, messageBody, stackTrace, color,
                    withScreenshot, cancellationToken);
            }
        }

        private async UniTask PostReportAsync(
            string slackChannel,
            string mention,
            bool here,
            string lead,
            string message,
            string stackTrace,
            Color color,
            bool withScreenshot,
            CancellationToken cancellationToken = default
        )
        {
            await _sender.Send(
                slackToken,
                slackChannel,
                mention.Split(','),
                here,
                lead,
                message,
                stackTrace,
                color,
                withScreenshot,
                cancellationToken
            );
            Logger?.Log($"Slack message sent to {slackChannel}");
        }

        private void OverwriteByCommandlineArguments()
        {
            var args = new Arguments();

            if (args.SlackToken.IsCaptured())
            {
                slackToken = args.SlackToken.Value();
            }

            if (args.SlackChannels.IsCaptured())
            {
                slackChannels = args.SlackChannels.Value();
            }
        }
    }
}
