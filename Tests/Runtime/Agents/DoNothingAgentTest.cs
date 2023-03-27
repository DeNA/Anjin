// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Utilities;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DeNA.Anjin.Agents
{
    public class DoNothingAgentTest
    {
        [Test]
        public async Task Run_cancelTask_stopAgent()
        {
            var agent = ScriptableObject.CreateInstance<DoNothingAgent>();
            agent.Logger = new ConsoleLogger(Debug.unityLogger.logHandler);
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = nameof(Run_cancelTask_stopAgent);
            agent.lifespanSec = 0; // Expect indefinite execution

            var agentName = agent.GetType().Name;
            var gameObject = new GameObject(agentName);
            var token = gameObject.GetCancellationTokenOnDestroy();
            var task = agent.Run(token);
            await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);

            Object.DestroyImmediate(gameObject);
            // ReSharper disable once MethodSupportsCancellation
            await UniTask.NextFrame();

            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Canceled));
        }

        [Test]
        public async Task Run_lifespanPassed_stopAgent()
        {
            var agent = ScriptableObject.CreateInstance<DoNothingAgent>();
            agent.Logger = new ConsoleLogger(Debug.unityLogger.logHandler);
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = nameof(Run_lifespanPassed_stopAgent);
            agent.lifespanSec = 1;

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var token = cancellationTokenSource.Token;
                var task = agent.Run(token);
                await UniTask.Delay(TimeSpan.FromSeconds(2), cancellationToken: token); // Consider overhead

                Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Succeeded));
            }
        }
    }
}
