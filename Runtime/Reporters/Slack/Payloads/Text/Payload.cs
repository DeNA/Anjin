// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using UnityEngine;

namespace DeNA.Anjin.Reporters.Slack.Payloads.Text
{
    /// <summary>
    /// Slack post payload.
    /// </summary>
    /// <see href="https://api.slack.com/web"/>
    [Serializable]
    public class Payload
    {
        public string channel;
        public string text;
        public Attachment[] attachments;

        // ReSharper disable InconsistentNaming
        public readonly string link_names = "1";
        public string thread_ts;
        // ReSharper restore InconsistentNaming

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }

        public static Payload CreatePayload(string channel, string text, Attachment[] attachments, string ts = null)
        {
            return new Payload { channel = channel, text = text, attachments = attachments, thread_ts = ts };
        }
    }
}
