// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DeNA.Anjin.Reporters
{
    /// <summary>
    /// An interface for Slack message senders. The derived class of this interface must define message format, and
    /// delegate touching Slack API to <c cref="SlackAPI" />.
    /// Purpose of introducing this interface is making <c cref="SlackReporter" /> be a humble object.
    /// </summary>
    public interface ISlackMessageSender
    {
        /// <summary>
        /// Post report log message, stacktrace and screenshot
        /// </summary>
        /// <param name="slackToken">Slack API token</param>
        /// <param name="slackChannel">Slack Channel to send notification</param>
        /// <param name="mentionSubTeamIDs">Sub team IDs to mention</param>
        /// <param name="addHereInSlackMessage">Whether adding @here or not</param>
        /// <param name="logString">Log message</param>
        /// <param name="stackTrace">Stack trace</param>
        /// <param name="withScreenshot">With screenshot</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        UniTask Send(
            string slackToken,
            string slackChannel,
            IEnumerable<string> mentionSubTeamIDs,
            bool addHereInSlackMessage,
            string logString,
            string stackTrace,
            bool withScreenshot,
            CancellationToken cancellationToken = default
        );
    }

    /// <summary>
    /// A class for Slack message senders. This class defines a message format and delegates touching Slack API to
    /// <c cref="SlackAPI" />.
    /// </summary>
    public class SlackMessageSender : ISlackMessageSender
    {
        private readonly SlackAPI _slackAPI;
        private readonly string _slackToken;


        /// <summary>
        /// Creates a new instance for <c cref="SlackMessageSender" />.
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
            string logString,
            string stackTrace,
            bool withScreenshot,
            CancellationToken cancellationToken = default
        )
        {
            if (string.IsNullOrEmpty(slackToken) || string.IsNullOrEmpty(slackChannel))
            {
                return;
            }

            var title = CreateTitle(logString, mentionSubTeamIDs, addHereInSlackMessage);

            await UniTask.SwitchToMainThread();

            var postTitleTask = await _slackAPI.Post(
                slackToken,
                slackChannel,
                title,
                cancellationToken: cancellationToken
            );
            if (!postTitleTask.Success)
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

                var postScreenshotTask = await _slackAPI.Post(
                    slackToken,
                    slackChannel,
                    withoutAlpha.EncodeToPNG(),
                    postTitleTask.Ts,
                    cancellationToken
                );
                if (!postScreenshotTask.Success)
                {
                    return;
                }
            }

            if (stackTrace != null && stackTrace.Length > 0)
            {
                var body = CreateStackTrace(stackTrace);
                await _slackAPI.Post(slackToken, slackChannel, body, postTitleTask.Ts, cancellationToken);
            }
        }


        private static string CreateTitle(string logString, IEnumerable<string> mentionSubTeamIDs, bool withHere)
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

            sb.Append(logString);
            return sb.ToString();
        }


        private static string CreateStackTrace( string stackTrace)
        {
            return $"```{stackTrace}```";
            // TODO: Split every 4k characters and quote.
        }

        private class CoroutineRunner : MonoBehaviour
        {
        }
    }
}
