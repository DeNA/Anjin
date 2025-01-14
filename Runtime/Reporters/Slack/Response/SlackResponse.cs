// Copyright (c) 2023-2025 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using UnityEngine;

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
