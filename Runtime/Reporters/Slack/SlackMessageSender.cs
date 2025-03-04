// Copyright (c) 2023-2025 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DeNA.Anjin.Reporters.Slack
{
    /// <summary>
    /// An interface for Slack message senders. The derived class of this interface must define message format, and
    /// delegate touching Slack API to <c cref="SlackAPI">SlackAPI</c>.
    /// Purpose of introducing this interface is making <c cref="SlackReporter">SlackReporter</c> be a humble object.
    /// </summary>
    public interface ISlackMessageSender
    {
        /// <summary>
        /// Post report log message, stacktrace and screenshot
        /// </summary>
        /// <param name="slackToken">Slack API token</param>
        /// <param name="slackChannel">Slack channel ID or name to post</param>
        /// <param name="mentionSubTeamIDs">Subteam IDs to mention</param>
        /// <param name="addHereInSlackMessage">Whether adding @here or not</param>
        /// <param name="lead">Lead text (out of attachment)</param>
        /// <param name="message">Message body text (into attachment)</param>
        /// <param name="stackTrace">Stack trace</param>
        /// <param name="color">Attachment color</param>
        /// <param name="withScreenshot">With screenshot</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        UniTask Send(
            string slackToken,
            string slackChannel,
            IEnumerable<string> mentionSubTeamIDs,
            bool addHereInSlackMessage,
            string lead,
            string message,
            string stackTrace,
            Color color,
            bool withScreenshot,
            CancellationToken cancellationToken = default
        );
    }

    /// <summary>
    /// A class for Slack message senders. This class defines a message format and delegates touching Slack API to
    /// <c cref="SlackAPI">SlackAPI</c>.
    /// </summary>
    public class SlackMessageSender : ISlackMessageSender
    {
        private readonly SlackAPI _slackAPI;
        private readonly string _slackToken;

        /// <summary>
        /// Creates a new instance for <c cref="SlackMessageSender">SlackMessageSender</c>.
        /// </summary>
        /// <param name="api">Slack API client</param>
        public SlackMessageSender(SlackAPI api)
        {
            _slackAPI = api;
        }

        /// <inheritdoc />
        public async UniTask Send(
            string slackToken,
            string slackChannel,
            IEnumerable<string> mentionSubTeamIDs,
            bool addHereInSlackMessage,
            string lead,
            string message,
            string stackTrace,
            Color color,
            bool withScreenshot,
            CancellationToken cancellationToken = default
        )
        {
            if (string.IsNullOrEmpty(slackToken) || string.IsNullOrEmpty(slackChannel))
            {
                return;
            }

            var text = CreateLead(lead, mentionSubTeamIDs, addHereInSlackMessage);

            await UniTask.SwitchToMainThread();

            var postTitleTask = await _slackAPI.Post(
                slackToken,
                slackChannel,
                text,
                message,
                color
            );
            if (!postTitleTask.ok)
            {
                return;
            }

            if (withScreenshot)
            {
                var coroutineRunner = new GameObject().AddComponent<CoroutineRunner>();
                await UniTask.WaitForEndOfFrame(coroutineRunner, cancellationToken);
                Object.Destroy(coroutineRunner);

                var screenshot = ScreenCapture.CaptureScreenshotAsTexture();
                var withoutAlpha = new Texture2D(screenshot.width, screenshot.height, TextureFormat.RGB24, false);
                withoutAlpha.SetPixels(screenshot.GetPixels());
                withoutAlpha.Apply();

                await _slackAPI.Post(
                    slackToken,
                    postTitleTask.channel, // channel ID
                    withoutAlpha.EncodeToPNG(),
                    postTitleTask.ts
                );
                // Note: Continue if screenshot submission fails.
            }

            if (!string.IsNullOrEmpty(stackTrace))
            {
                await _slackAPI.PostWithoutAttachments(slackToken, slackChannel, stackTrace, postTitleTask.ts);
            }
        }

        private static string CreateLead(string lead, IEnumerable<string> mentionSubTeamIDs, bool withHere)
        {
            var sb = new StringBuilder();
            foreach (var s in mentionSubTeamIDs)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    // ReSharper disable once StringLiteralTypo
                    sb.Append($"<!subteam^{s}> ");
                }
            }

            if (withHere)
            {
                sb.Append("<!here> ");
            }

            if (!string.IsNullOrEmpty(lead))
            {
                sb.Append(lead);
            }

            return sb.ToString();
        }

        private class CoroutineRunner : MonoBehaviour
        {
        }
    }
}
