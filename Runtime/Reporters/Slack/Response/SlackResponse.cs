// Copyright (c) 2023-2025 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using UnityEngine;
using UnityEngine.Networking;

namespace DeNA.Anjin.Reporters.Slack.Response
{
    /// <summary>
    /// Response of general or chat.postMessage.
    /// </summary>
    /// <see href="https://api.slack.com/web#responses"/>
    /// <see href="https://api.slack.com/methods/chat.postMessage"/>
    [Serializable]
    public struct SlackResponse
    {
        /// <summary>
        /// Success or failure.
        /// </summary>
        public bool ok;

        /// <summary>
        /// Short machine-readable error code.
        /// </summary>
        public string error;

        /// <summary>
        /// Short machine-readable warning code (or comma-separated list of them, in the case of multiple warnings).
        /// </summary>
        public string warning;

        /// <summary>
        /// Thread timestamp.
        /// </summary>
        public string ts;

        /// <summary>
        /// Constructor for tests.
        /// </summary>
        /// <param name="ok">Post was success</param>
        /// <param name="ts">Thread timestamp</param>
        [Obsolete]
        public SlackResponse(bool ok, string ts = null)
        {
            this.ok = ok;
            this.error = null;
            this.warning = null;
            this.ts = ts;
        }

        /// <summary>
        /// Create instance from <c>GetUploadURLExternalResponse</c>.
        /// </summary>
        /// <param name="source">Source response instance</param>
        /// <returns></returns>
        public static SlackResponse FromGetUploadURLExternalResponse(GetUploadURLExternalResponse source)
        {
            return new SlackResponse() { ok = source.ok, error = source.error, warning = source.warning, ts = null };
        }

        /// <summary>
        /// Create instance from <c>UnityWebRequest</c>.
        /// </summary>
        /// <param name="www">UnityWebRequest with response</param>
        /// <param name="parseBody">Parse web-response body. if false, always returns success response</param>
        /// <returns></returns>
        public static SlackResponse FromWebResponse(UnityWebRequest www, bool parseBody = true)
        {
#if UNITY_2020_2_OR_NEWER
            if (www.result != UnityWebRequest.Result.Success)
            {
                return new SlackResponse() { ok = false };
            }
#else
            if (www.isNetworkError || www.isHttpError)
            {
                return new SlackResponse() { ok = false };
            }
#endif

            if (parseBody)
            {
                return FromJson(www.downloadHandler.text);
            }

            return new SlackResponse() { ok = true };
        }

        public static SlackResponse FromJson(string json)
        {
            return JsonUtility.FromJson<SlackResponse>(json);
        }

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }
    }
}
