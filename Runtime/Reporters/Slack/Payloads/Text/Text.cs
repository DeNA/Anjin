// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;

namespace DeNA.Anjin.Reporters.Slack.Payloads.Text
{
    /// <summary>
    /// Text object.
    /// </summary>
    /// <remarks>
    /// Abstract classes and interfaces are not available. <c>JsonUtility.ToJson</c> can not process them.
    /// </remarks>
    /// <see href="https://api.slack.com/reference/block-kit/composition-objects#text"/>
    [Serializable]
    public class Text
    {
        public string type; // "plain_text" or "mrkdwn"
        public string text;

        // Note: bool emoji is only usable when `type` is "plain_text".
        // Note: bool verbatim is only usable when `type` is "mrkdwn".

        public static Text CreatePlainText(string text)
        {
            return new Text { type = "plain_text", text = text };
        }

        public static Text CreateMarkdownText(string text)
        {
            return new Text { type = "mrkdwn", text = text };
        }
    }
}
