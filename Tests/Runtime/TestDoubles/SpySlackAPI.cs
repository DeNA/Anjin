// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Reporters.Slack;
using DeNA.Anjin.Reporters.Slack.Response;
using UnityEngine;

namespace DeNA.Anjin.TestDoubles
{
    public class SpySlackAPI : SlackAPI
    {
        public List<Dictionary<string, string>> Arguments { get; } = new List<Dictionary<string, string>>();

        public override async UniTask<SlackResponse> Post(string token, string channel, string text, string message,
            Color color, string ts = null)
        {
            var args = new Dictionary<string, string>();
            args.Add("token", token);
            args.Add("channel", channel);
            args.Add("text", text);
            args.Add("message", message);
            args.Add("color", color.ToString());
            args.Add("ts", ts);
            Arguments.Add(args);

            await UniTask.NextFrame();

            return new SlackResponse(true, "1");
        }

        public override async UniTask<SlackResponse> PostWithoutAttachments(string token, string channel, string text,
            string ts = null)
        {
            var args = new Dictionary<string, string>();
            args.Add("token", token);
            args.Add("channel", channel);
            args.Add("text", text);
            args.Add("ts", ts);
            Arguments.Add(args);

            await UniTask.NextFrame();

            return new SlackResponse(true, "1");
        }

        public override async UniTask<SlackResponse> Post(string token, string channel, byte[] image,
            string ts = null)
        {
            return await Post(token, channel, null, "IMAGE", default, ts);
        }
    }
}
