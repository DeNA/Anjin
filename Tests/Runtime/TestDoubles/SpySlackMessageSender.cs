// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Reporters.Slack;
using UnityEngine;

namespace DeNA.Anjin.TestDoubles
{
    public class SpySlackMessageSender : ISlackMessageSender
    {
        public struct CallArguments
        {
            public string SlackToken { get; set; }
            public string SlackChannel { get; set; }
            public IEnumerable<string> MentionSubTeamIDs { get; set; }
            public bool AddHereInSlackMessage { get; set; }
            public string Lead { get; set; }
            public string Message { get; set; }
            public string StackTrace { get; set; }
            public Color Color { get; set; }
            public bool WithScreenshot { get; set; }
        }

        public List<CallArguments> CalledList { get; } = new List<CallArguments>();

        public async UniTask Send(string slackToken, string slackChannel, IEnumerable<string> mentionSubTeamIDs,
            bool addHereInSlackMessage, string lead, string message, string stackTrace, Color color,
            bool withScreenshot, CancellationToken cancellationToken = default)
        {
            var called = new CallArguments
            {
                SlackToken = slackToken,
                SlackChannel = slackChannel,
                MentionSubTeamIDs = mentionSubTeamIDs,
                AddHereInSlackMessage = addHereInSlackMessage,
                Lead = lead,
                Message = message,
                StackTrace = stackTrace,
                Color = color,
                WithScreenshot = withScreenshot,
            };
            CalledList.Add(called);
            await UniTask.CompletedTask;
        }
    }
}
