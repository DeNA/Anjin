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
    [CreateAssetMenu(fileName = "New SlackReporter", menuName = "Anjin/Slack Reporter", order = 50)]
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
        /// With take a screenshot or not (on error terminates).
        /// </summary>
        public bool withScreenshotOnError = true;

        /// <summary>
        /// Post a report if normally terminates.
        /// </summary>
        public bool postOnNormally;

        /// <summary>
        /// With take a screenshot or not (on normally terminates).
        /// </summary>
        public bool withScreenshotOnNormally;

        private readonly ISlackMessageSender _sender = new SlackMessageSender(new SlackAPI());

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

            var withScreenshot = exitCode == ExitCode.Normally ? withScreenshotOnNormally : withScreenshotOnError;
            // TODO: build message body with template and placeholders

            OverwriteByCommandlineArguments();
            // TODO: log warn if slackToken or slackChannels is empty

            // NOTE: In _sender.send, switch the execution thread to the main thread, so UniTask.WhenAll is meaningless.
            foreach (var slackChannel in slackChannels.Split(','))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                await PostReportAsync(slackChannel, message, stackTrace, withScreenshot, cancellationToken);
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
            // TODO: can log slack post url?
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
