// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Reporters;

namespace DeNA.Anjin.TestDoubles
{
    public class SpySlackAPI : SlackAPI
    {
        public List<Dictionary<string, string>> Arguments { get; } = new List<Dictionary<string, string>>();

        public override async UniTask<SlackResponse> Post(string token, string channel, string text,
            string ts = null, CancellationToken cancellationToken = default)
        {
            var args = new Dictionary<string, string>();
            args.Add("token", token);
            args.Add("channel", channel);
            args.Add("message", text);
            args.Add("ts", ts);
            Arguments.Add(args);

            await UniTask.NextFrame(cancellationToken);

            return new SlackResponse(true, "1");
        }

        public override async UniTask<SlackResponse> Post(string token, string channel, byte[] image,
            string ts = null, CancellationToken cancellationToken = default)
        {
            return await Post(token, channel, "IMAGE", ts, cancellationToken);
        }
    }
}
