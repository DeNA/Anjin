// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Reporters.Slack.Payloads.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace DeNA.Anjin.Reporters.Slack
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
        /// <param name="text">Lead text (out of attachment)</param>
        /// <param name="message">Message body text (into attachment)</param>
        /// <param name="color">Attachment color</param>
        /// <param name="ts">Thread timestamp</param>
        /// <returns></returns>
        public virtual async UniTask<SlackResponse> Post(string token, string channel, string text, string message,
            Color color, string ts = null)
        {
            const string URL = URLBase + "chat.postMessage";
            var payload = Payload.CreatePayload(
                channel,
                text,
                new[]
                {
                    Attachment.CreateAttachment(
                        new[]
                        {
                            ContextBlock.CreateContextBlock(
                                new[] { Text.CreateMarkdownText(message) }
                            )
                        },
                        color
                    )
                },
                ts
            );

            return await Post(URL, token, payload);
        }

        /// <summary>
        /// Post text message to thread.
        /// Without attachments.
        /// </summary>
        /// <param name="token">Slack token</param>
        /// <param name="channel">Send target channels</param>
        /// <param name="text">Text (out of attachment)</param>
        /// <param name="ts">Thread timestamp</param>
        /// <returns></returns>
        public virtual async UniTask<SlackResponse> PostWithoutAttachments(string token, string channel, string text,
            string ts = null)
        {
            const string URL = URLBase + "chat.postMessage";
            var payload = Payload.CreatePayload(
                channel,
                text,
                null,
                ts
            );

            return await Post(URL, token, payload);
        }

        /// <summary>
        /// Post image
        /// </summary>
        /// <param name="token">Slack token</param>
        /// <param name="channel">Send target channels</param>
        /// <param name="image">Image (screenshot)</param>
        /// <param name="ts">Thread timestamp</param>
        /// <returns></returns>
        public virtual async UniTask<SlackResponse> Post(string token, string channel, byte[] image,
            string ts = null)
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

        [Obsolete]
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

        private static async UniTask<SlackResponse> Post(string url, string token, Payload payload)
        {
#if UNITY_2022_2_OR_NEWER
            using (var www = UnityWebRequest.Post(url, payload.ToJson(), "application/json; charset=utf-8"))
            {
#else
            using (var www = UnityWebRequest.Post(url, payload.ToJson()))
            {
                www.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
#endif
                www.SetRequestHeader("Authorization", $"Bearer {token}");
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
