// Copyright (c) 2023 DeNA Co., Ltd.
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

        private readonly ISlackMessageSender _sender = new SlackMessageSender(new SlackAPI());

        /// <inheritdoc />
        public override async UniTask PostReportAsync(
            AutopilotSettings settings,
            string logString,
            string stackTrace,
            LogType type,
            bool withScreenshot,
            CancellationToken cancellationToken = default
        )
        {
            // NOTE: In _sender.send, switch the execution thread to the main thread, so UniTask.WhenAll is meaningless.
            foreach (var slackChannel in (string.IsNullOrEmpty(settings.slackChannels)
                         ? slackChannels
                         : settings.slackChannels).Split(','))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                
                await _sender.Send(
                    string.IsNullOrEmpty(settings.slackToken) ? slackToken : settings.slackToken,
                    slackChannel,
                    mentionSubTeamIDs.Split(','),
                    addHereInSlackMessage,
                    logString,
                    stackTrace,
                    withScreenshot,
                    cancellationToken
                );
            }
        }
    }
}
