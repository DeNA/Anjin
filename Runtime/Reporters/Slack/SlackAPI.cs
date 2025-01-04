// Copyright (c) 2023-2025 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Reporters.Slack.Payloads.CompleteUploadExternal;
using DeNA.Anjin.Reporters.Slack.Payloads.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace DeNA.Anjin.Reporters.Slack
{
    /// <summary>
    /// Slack API interface
    /// </summary>
    public class SlackAPI
    {
        private const string URLBase = "https://slack.com/api/";
        private static bool IsSuccess(string text) => text.Contains("\"ok\":true");

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
        public virtual async UniTask<SlackResponse> Post(string token, string channel, byte[] image, string ts = null)
        {
            const string Filename = "screenshot.png";

            var uploadURLExternalResponse = await GetUploadURLExternal(token, Filename, image.Length);
            if (!uploadURLExternalResponse.Success)
            {
                return new SlackResponse(false);
            }

            var uploadResponse = await UploadImage(uploadURLExternalResponse.UploadUrl, Filename, image);
            if (!uploadResponse.Success)
            {
                return new SlackResponse(false);
            }

            return await CompleteUploadExternal(token, uploadURLExternalResponse.FileId, channel, ts);
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
                    return new SlackResponse(false);
                }
#else
                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.LogWarning($"{www.responseCode}");
                    return new SlackResponse(false);
                }
#endif

                if (!IsSuccess(www.downloadHandler.text))
                {
                    Debug.LogWarning($"{www.downloadHandler.text}");
                    return new SlackResponse(false);
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

        private static async UniTask<GetUploadURLExternalResponse> GetUploadURLExternal(string token, string filename,
            int length)
        {
            var url = $"{URLBase}/files.getUploadURLExternal?filename={filename}&length={length}";

            using (var www = UnityWebRequest.Get(url))
            {
                www.SetRequestHeader("Authorization", $"Bearer {token}");
                await www.SendWebRequest();
#if UNITY_2020_2_OR_NEWER
                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"{www.responseCode}");
                    return new GetUploadURLExternalResponse(false);
                }
#else
                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.LogWarning($"{www.responseCode}");
                    return new GetUploadURLExternalResponse(false);
                }
#endif

                var response = new GetUploadURLExternalResponse();
                if (!response.ParseResponse(www.downloadHandler.text))
                {
                    Debug.LogWarning($"{www.downloadHandler.text}");
                }

                return response;
            }
        }

        private static async UniTask<SlackResponse> UploadImage(string url, string filename, byte[] image)
        {
            var form = new WWWForm();
            form.AddBinaryData("file", image, filename, "image/png");

            using (var www = UnityWebRequest.Post(url, form))
            {
                await www.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"{www.responseCode}");
                    return new SlackResponse(false);
                }
#else
                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.LogWarning($"{www.responseCode}");
                    return new SlackResponse(false);
                }
#endif

                return new SlackResponse(true);
                // Note: Slack will return HTTP 200 if the upload is successful; a non-200 response indicates a failure.
            }
        }

        private static async UniTask<SlackResponse> CompleteUploadExternal(string token, string fileId,
            string channelId, string threadTs)
        {
            var url = $"{URLBase}/files.completeUploadExternal";
            var arguments = new CompleteUploadExternalArguments(fileId, channelId, threadTs);

#if UNITY_2022_2_OR_NEWER
            using (var www = UnityWebRequest.Post(url, arguments.ToJson(), "application/json; charset=utf-8"))
            {
#else
            using (var www = UnityWebRequest.Post(url, arguments.ToJson()))
            {
                www.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
#endif
                www.SetRequestHeader("Authorization", $"Bearer {token}");
                await www.SendWebRequest();

#if UNITY_2020_2_OR_NEWER
                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"{www.responseCode}");
                    return new SlackResponse(false);
                }
#else
                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.LogWarning($"{www.responseCode}");
                    return new SlackResponse(false);
                }
#endif

                if (!IsSuccess(www.downloadHandler.text))
                {
                    Debug.LogWarning($"{www.downloadHandler.text}");
                    return new SlackResponse(false);
                }

                return new SlackResponse(true);
            }
        }
    }
}
