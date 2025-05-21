// Copyright (c) 2023-2025 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Settings;
using DeNA.Anjin.Strategies;
using DeNA.Anjin.TestDoubles;
using DeNA.Anjin.Utilities;
using NUnit.Framework;
using TestHelper.Attributes;
using UnityEngine;
using UnityEngine.TestTools;

namespace DeNA.Anjin.Agents
{
    public class UGUIMonkeyAgentTest
    {
        private const string TestScene = "Packages/com.dena.anjin/Tests/TestScenes/Buttons.unity";

        private static UGUIMonkeyAgent CreateAgent()
        {
            var agent = ScriptableObject.CreateInstance<UGUIMonkeyAgent>();
            agent.name = TestContext.CurrentContext.Test.Name;
            agent.Logger = Debug.unityLogger;
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.delayMillis = 100;
            agent.bufferLengthForDetectLooping = 0; // Disable loop detection
            agent.touchAndHoldDelayMillis = 100;
            return agent;
        }

        [Test]
        [LoadScene(TestScene)]
        [Timeout(5000)]
        [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
        public async Task Run_CancelTask_StopAgent()
        {
            var agent = CreateAgent();
            agent.lifespanSec = 0; // Expect indefinite execution

            using (var cts = new CancellationTokenSource())
            {
                var cancellationToken = cts.Token;
                var task = agent.Run(cancellationToken);
                await UniTask.NextFrame();

                cts.Cancel();
                await UniTask.NextFrame();

                Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Canceled));
            }

            LogAssert.Expect(LogType.Log, $"Enter {agent.name}.Run()");
            LogAssert.Expect(LogType.Log, $"Exit {agent.name}.Run()");
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task Run_LifespanPassed_StopAgent()
        {
            var agent = CreateAgent();
            agent.lifespanSec = 1;

            var task = agent.Run(CancellationToken.None);
            await UniTask.Delay(2000); // Consider overhead

            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Succeeded));

            LogAssert.Expect(LogType.Log, $"Enter {agent.name}.Run()");
            LogAssert.Expect(LogType.Log, $"Exit {agent.name}.Run()");
        }

        [Test]
        [FocusGameView]
        [LoadScene(TestScene)]
        public async Task Run_DefaultScreenshotFilenamePrefix_UseAgentName()
        {
            var outputDir = Path.Combine(Application.temporaryCachePath, TestContext.CurrentContext.Test.ClassName);
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            settings.outputRootPath = outputDir;
            AutopilotState.Instance.settings = settings;

            var agentName = TestContext.CurrentContext.Test.Name;
            var filename = $"{agentName}01_0001.png";
            var path = Path.Combine(settings.ScreenshotsPath, filename);
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            Assume.That(path, Does.Not.Exist);

            var agent = CreateAgent();
            agent.lifespanSec = 1;
            agent.screenshotEnabled = true;
            agent.defaultScreenshotFilenamePrefix = true; // Use default prefix

            TwoTieredCounterStrategy.ResetPrefixCounters();

            await agent.Run(CancellationToken.None);

            Assert.That(path, Does.Exist);

            // teardown
            AutopilotState.Instance.Reset();
        }

        [Test]
        [LoadScene("Packages/com.dena.anjin/Tests/TestScenes/Empty.unity")]
        [Timeout(5000)]
        public async Task Run_TimeoutExceptionOccurred_AutopilotFailed()
        {
            var agent = CreateAgent();
            agent.lifespanSec = 0;                             // Expect indefinite execution
            agent.secondsToErrorForNoInteractiveComponent = 1; // TimeoutException occurred after 1 second

            var spyTerminatable = new SpyTerminatable();
            agent.AutopilotInstance = spyTerminatable;

            await agent.Run(CancellationToken.None);
            await UniTask.NextFrame();

            Assert.That(spyTerminatable.IsCalled, Is.True);
            Assert.That(spyTerminatable.CapturedExitCode, Is.EqualTo(ExitCode.AutopilotFailed));
            Assert.That(spyTerminatable.CapturedMessage, Does.StartWith(
                "TimeoutException: Interactive component not found in 1 seconds"));
            Assert.That(spyTerminatable.CapturedStackTrace, Does.StartWith(
                "  at TestHelper.Monkey.Monkey"));
            Assert.That(spyTerminatable.CapturedReporting, Is.True);

            LogAssert.Expect(LogType.Log, $"Enter {agent.name}.Run()");
            LogAssert.Expect(LogType.Log, $"Exit {agent.name}.Run()");
        }

        [Test]
        [LoadScene(TestScene)]
        [Timeout(10000)]
        public async Task Run_InfiniteLoopExceptionOccurred_AutopilotFailed()
        {
            var agent = CreateAgent();
            agent.lifespanSec = 0;                  // Expect indefinite execution
            agent.bufferLengthForDetectLooping = 5; // Enable infinite loop detection
            // Note: The loop is detected immediately because there are only two active buttons on the test scene.

            var spyTerminatable = new SpyTerminatable();
            agent.AutopilotInstance = spyTerminatable;

            await agent.Run(CancellationToken.None);
            await UniTask.NextFrame();

            Assert.That(spyTerminatable.IsCalled, Is.True);
            Assert.That(spyTerminatable.CapturedExitCode, Is.EqualTo(ExitCode.AutopilotFailed));
            Assert.That(spyTerminatable.CapturedMessage, Does.StartWith(
                "InfiniteLoopException: Found loop in the operation sequence"));
            Assert.That(spyTerminatable.CapturedStackTrace, Does.StartWith(
                "  at TestHelper.Monkey.Monkey"));
            Assert.That(spyTerminatable.CapturedReporting, Is.True);

            LogAssert.Expect(LogType.Log, $"Enter {agent.name}.Run()");
            LogAssert.Expect(LogType.Log, $"Exit {agent.name}.Run()");
        }
    }
}
