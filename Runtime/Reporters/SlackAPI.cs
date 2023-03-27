// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace DeNA.Anjin.Reporters
{
    /// <summary>
    /// Slack post message API response
    /// </summary>
    public struct SlackResponse
    {
        /// <summary>
        /// Success/failure
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Thread timestamp
        /// </summary>
        public string Ts { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="success">Post was success</param>
        /// <param name="ts">Thread timestamp</param>
        public SlackResponse(bool success, string ts)
        {
            Success = success;
            Ts = ts;
        }
    }

    /// <summary>
    /// Slack API interface
    /// </summary>
    public class SlackAPI
    {
        private const string URLBase = "https://slack.com/api/";

        /// <summary>
        /// Post text message
        /// </summary>
        /// <param name="token">Slack token</param>
        /// <param name="channel">Send target channels</param>
        /// <param name="text">Message body</param>
        /// <param name="ts">Thread timestamp</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public virtual async UniTask<SlackResponse> Post(string token, string channel, string text,
            string ts = null, CancellationToken cancellationToken = default)
        {
            const string URL = URLBase + "chat.postMessage";
            var form = new WWWForm();
            form.AddField("token", token);
            form.AddField("channel", channel);
            form.AddField("text", text);
            form.AddField("link_names", "1");
            if (ts != null)
            {
                form.AddField("thread_ts", ts);
            }

            return await Post(URL, form);
        }

        /// <summary>
        /// Post image
        /// </summary>
        /// <param name="token">Slack token</param>
        /// <param name="channel">Send target channels</param>
        /// <param name="image">Image (screenshot)</param>
        /// <param name="ts">Thread timestamp</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public virtual async UniTask<SlackResponse> Post(string token, string channel, byte[] image,
            string ts = null, CancellationToken cancellationToken = default)
        {
            const string URL = URLBase + "files.upload";
            var form = new WWWForm();
            form.AddField("token", token);
            form.AddField("channels", channel);
            form.AddBinaryData("file", image, "image.png", "image/png");
            if (ts != null)
            {
                form.AddField("thread_ts", ts);
            }

            return await Post(URL, form);
        }

        private static async UniTask<SlackResponse> Post(string url, WWWForm form)
        {
            using (var www = UnityWebRequest.Post(url, form))
            {
                await www.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"{www.responseCode}");
                    return new SlackResponse(false, null);
                }
#else
                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.LogWarning($"{www.responseCode}");
                    return new SlackResponse(false, null);
                }
#endif

                if (www.downloadHandler.text.Contains("\"ok\":false"))
                {
                    Debug.LogWarning($"{www.downloadHandler.text}");
                    return new SlackResponse(false, null);
                }

                string ts = null;
                var tsMatch = new Regex("\"ts\":\"(\\d+\\.\\d+)\"").Match(www.downloadHandler.text);
                if (tsMatch.Success && tsMatch.Length > 0)
                {
                    ts = tsMatch.Groups[1].Value;
                }

                return new SlackResponse(true, ts);
            }
        }
    }
}
