// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Text;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Settings;
using UnityEngine;

namespace DeNA.Anjin.Reporters
{
    /// <summary>
    /// Post report to Slack
    /// </summary>
    public class SlackReporter : IReporter
    {
        private readonly AutopilotSettings _settings;
        private readonly SlackAPI _slackAPI;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="slackAPI"></param>
        public SlackReporter(AutopilotSettings settings, SlackAPI slackAPI)
        {
            _settings = settings;
            _slackAPI = slackAPI;
        }

        /// <summary>
        /// Post report log message, stacktrace and screenshot to Slack
        /// </summary>
        /// <param name="logString">Log message string</param>
        /// <param name="stackTrace">Stack trace</param>
        /// <param name="type">Log message type</param>
        /// <param name="withScreenshot">With screenshot</param>
        /// <returns></returns>
        public async UniTask PostReportAsync(string logString, string stackTrace, LogType type, bool withScreenshot)
        {
            if (string.IsNullOrEmpty(_settings.slackToken) || string.IsNullOrEmpty(_settings.slackChannels))
            {
                return;
            }

            var title = Title(logString, _settings.mentionSubTeamIDs, _settings.addHereInSlackMessage);
            var body = Body(logString, stackTrace);

            await UniTask.SwitchToMainThread();

            foreach (var channel in _settings.slackChannels.Split(','))
            {
                if (string.IsNullOrEmpty(channel))
                {
                    continue;
                }

                var postTitleTask = await _slackAPI.Post(_settings.slackToken, channel, title);
                if (!postTitleTask.Success)
                {
                    return;
                }

                if (withScreenshot && !Application.isBatchMode)
                {
                    var coroutineRunner = new GameObject().AddComponent<CoroutineRunner>();
                    await UniTask.WaitForEndOfFrame(coroutineRunner);
                    Object.Destroy(coroutineRunner);

                    var screenshot = ScreenCapture.CaptureScreenshotAsTexture();
                    var withoutAlpha = new Texture2D(screenshot.width, screenshot.height, TextureFormat.RGB24, false);
                    withoutAlpha.SetPixels(screenshot.GetPixels());
                    withoutAlpha.Apply();

                    var postScreenshotTask = await _slackAPI.Post(_settings.slackToken, channel,
                        withoutAlpha.EncodeToPNG(), postTitleTask.Ts);
                    if (!postScreenshotTask.Success)
                    {
                        return;
                    }
                }

                var postBodyTask = await _slackAPI.Post(_settings.slackToken, channel, body,
                    postTitleTask.Ts);
                if (!postBodyTask.Success)
                {
                    return;
                }
            }
        }

        private static string Title(string logString, string mentionSubTeamIDs, bool withHere)
        {
            var title = new StringBuilder();

            foreach (var s in mentionSubTeamIDs.Split(','))
            {
                if (!string.IsNullOrEmpty(s))
                {
                    // ReSharper disable once StringLiteralTypo
                    title.Append($"<!subteam^{s}> ");
                }
            }

            if (withHere)
            {
                title.Append("<!here> ");
            }

            return title.Append(logString).ToString();
        }

        private static string Body(string logString, string stackTrace)
        {
            return $"{logString}\n\n```{stackTrace}```";
        }

        private class CoroutineRunner : MonoBehaviour
        {
        }
    }
}
