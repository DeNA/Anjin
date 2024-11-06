// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using UnityEngine;

namespace DeNA.Anjin.Reporters.Slack.Payloads.Text
{
    /// <summary>
    /// Attachment.
    /// </summary>
    /// <see href="https://api.slack.com/reference/messaging/attachments"/>
    [Serializable]
    public class Attachment
    {
        public ContextBlock[] blocks;
        public string color; // e.g., "#36a64f"

        public static Attachment CreateAttachment(ContextBlock[] blocks, Color color)
        {
            return new Attachment { blocks = blocks, color = ColorToString(color) };
        }

        private static string ColorToString(Color color)
        {
            var r = ((int)(color.r * 255)).ToString("x2");
            var g = ((int)(color.g * 255)).ToString("x2");
            var b = ((int)(color.b * 255)).ToString("x2");
            return $"#{r}{g}{b}";
        }
    }
}
