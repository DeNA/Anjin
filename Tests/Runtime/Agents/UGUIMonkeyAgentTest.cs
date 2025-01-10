// Copyright (c) 2023-2024 DeNA Co., Ltd.
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
using Object = UnityEngine.Object;

namespace DeNA.Anjin.Agents
{
    [SuppressMessage("ApiDesign", "RS0030")]
    public class UGUIMonkeyAgentTest
    {
        private const string TestScene = "Packages/com.dena.anjin/Tests/TestScenes/Buttons.unity";

        [Test]
        [LoadScene(TestScene)]
        public async Task Run_CancelTask_StopAgent()
        {
            var agent = ScriptableObject.CreateInstance<UGUIMonkeyAgent>();
            agent.Logger = Debug.unityLogger;
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = TestContext.CurrentContext.Test.Name;
            agent.lifespanSec = 0; // Expect indefinite execution
            agent.delayMillis = 100;

            var gameObject = new GameObject();
            var cancellationToken = gameObject.GetCancellationTokenOnDestroy();
            var task = agent.Run(cancellationToken);
            await UniTask.NextFrame();

            Object.DestroyImmediate(gameObject);
            await UniTask.NextFrame();

            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Canceled));

            LogAssert.Expect(LogType.Log, $"Enter {agent.name}.Run()");
            LogAssert.Expect(LogType.Log, $"Exit {agent.name}.Run()");
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task Run_LifespanPassed_StopAgent()
        {
            var agent = ScriptableObject.CreateInstance<UGUIMonkeyAgent>();
            agent.Logger = Debug.unityLogger;
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = TestContext.CurrentContext.Test.Name;
            agent.lifespanSec = 1;
            agent.delayMillis = 100;

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var cancellationToken = cancellationTokenSource.Token;
                var task = agent.Run(cancellationToken);
                await UniTask.Delay(2000); // Consider overhead

                Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Succeeded));
            }

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

            var agent = ScriptableObject.CreateInstance<UGUIMonkeyAgent>();
            agent.Logger = Debug.unityLogger;
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = agentName;
            agent.lifespanSec = 1;
            agent.delayMillis = 100;
            agent.touchAndHoldDelayMillis = 100;
            agent.screenshotEnabled = true;
            agent.defaultScreenshotFilenamePrefix = true; // Use default prefix

            TwoTieredCounterStrategy.ResetPrefixCounters();

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var cancellationToken = cancellationTokenSource.Token;
                await agent.Run(cancellationToken);
            }

            Assert.That(path, Does.Exist);

            // teardown
            AutopilotState.Instance.Reset();
        }

        [Test]
        [FocusGameView]
        [LoadScene("Packages/com.dena.anjin/Tests/TestScenes/Empty.unity")]
        public async Task Run_TimeoutExceptionOccurred_AutopilotFailed()
        {
            var agent = ScriptableObject.CreateInstance<UGUIMonkeyAgent>();
            agent.Logger = Debug.unityLogger;
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = TestContext.CurrentContext.Test.Name;
            agent.lifespanSec = 5;
            agent.delayMillis = 100;
            agent.touchAndHoldDelayMillis = 100;
            agent.secondsToErrorForNoInteractiveComponent = 1; // TimeoutException occurred after 1 second

            var spyTerminatable = new SpyTerminatable();
            agent.AutopilotInstance = spyTerminatable;

            using (var cts = new CancellationTokenSource())
            {
                await agent.Run(cts.Token);
                await UniTask.NextFrame();
            }

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
    }
}
