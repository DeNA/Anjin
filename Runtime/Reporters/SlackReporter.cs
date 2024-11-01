// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Threading;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Settings;
using UnityEngine;

namespace DeNA.Anjin.Reporters
{
    /// <summary>
    /// Post report to Slack
    /// </summary>
    [CreateAssetMenu(fileName = "New SlackReporter", menuName = "Anjin/Slack Reporter", order = 90)]
    public class SlackReporter : AbstractReporter
    {
        /// <summary>
        /// Slack API token
        /// </summary>
        public string slackToken;

        /// <summary>
        /// Slack channels to send notification (comma separated)
        /// </summary>
        public string slackChannels;

        /// <summary>
        /// Sub team IDs to mention (comma separated)
        /// </summary>
        public string mentionSubTeamIDs;

        /// <summary>
        /// Whether adding @here or not
        /// </summary>
        public bool addHereInSlackMessage;

        /// <summary>
        /// Message body template (use on error terminates).
        /// </summary>
        [Multiline]
        public string messageBodyTemplateOnError = @"{message}";

        /// <summary>
        /// With take a screenshot or not (use on error terminates).
        /// </summary>
        public bool withScreenshotOnError = true;

        /// <summary>
        /// Post a report if normally terminates.
        /// </summary>
        public bool postOnNormally;

        /// <summary>
        /// Message body template (use on normally terminates).
        /// </summary>
        [Multiline]
        public string messageBodyTemplateOnNormally = @"{message}";

        /// <summary>
        /// With take a screenshot or not (use on normally terminates).
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

            var withScreenshot = (exitCode == ExitCode.Normally)
                ? withScreenshotOnNormally
                : withScreenshotOnError;
            var messageBody = MessageBuilder.BuildWithTemplate((exitCode == ExitCode.Normally)
                    ? messageBodyTemplateOnNormally
                    : messageBodyTemplateOnError,
                message);

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

                await PostReportAsync(slackChannel, messageBody, stackTrace, withScreenshot, cancellationToken);
            }
        }

        private async UniTask PostReportAsync(
            string slackChannel,
            string message,
            string stackTrace,
            bool withScreenshot,
            CancellationToken cancellationToken = default
        )
        {
            await _sender.Send(
                slackToken,
                slackChannel,
                mentionSubTeamIDs.Split(','),
                addHereInSlackMessage,
                message,
                stackTrace,
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
