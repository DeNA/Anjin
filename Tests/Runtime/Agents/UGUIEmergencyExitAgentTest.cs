// Copyright (c) 2023-2025 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Annotations;
using DeNA.Anjin.Settings;
using DeNA.Anjin.Strategies;
using DeNA.Anjin.TestDoubles;
using DeNA.Anjin.Utilities;
using NUnit.Framework;
using TestHelper.Attributes;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace DeNA.Anjin.Agents
{
    [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
    public class UGUIEmergencyExitAgentTest
    {
        private const string TestScene = "Packages/com.dena.anjin/Tests/TestScenes/Buttons.unity";

        private static UGUIEmergencyExitAgent CreateAgent()
        {
            var agent = ScriptableObject.CreateInstance<UGUIEmergencyExitAgent>();
            agent.name = TestContext.CurrentContext.Test.Name;
            agent.Logger = Debug.unityLogger;
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.intervalMillis = 100;
            return agent;
        }

        private static SpyButton AttachEmergencyExitAnnotation(
            bool buttonInteractable = true,
            bool annotationEnabled = true,
            bool reachable = true)
        {
            var button = GameObject.Find("Button 1");
            button.GetComponent<Button>().interactable = buttonInteractable;

            var annotation = button.AddComponent<EmergencyExitAnnotation>();
            annotation.enabled = annotationEnabled;

            if (!reachable)
            {
                button.GetComponent<RectTransform>().anchoredPosition = new Vector2(1000, 1000); // Move to out of sight
            }

            return button.gameObject.AddComponent<SpyButton>();
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task Run_CancelTask_StopAgent()
        {
            var agent = CreateAgent();

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var token = cancellationTokenSource.Token;
                var task = agent.Run(token);
                await UniTask.NextFrame();

                cancellationTokenSource.Cancel();
                await UniTask.NextFrame();

                Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Canceled));
            }

            LogAssert.Expect(LogType.Log, $"Enter {agent.name}.Run()");
            LogAssert.Expect(LogType.Log, $"Exit {agent.name}.Run()");
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task Run_ExistEmergencyExitButton_ClickEmergencyExitButton()
        {
            var agent = CreateAgent();

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var token = cancellationTokenSource.Token;
                var task = agent.Run(token);
                await UniTask.NextFrame();

                var emergencyButton = AttachEmergencyExitAnnotation();
                await UniTask.Delay(200);

                Assert.That(emergencyButton.IsClicked, Is.True, "Clicked button with EmergencyExit annotation");
                Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Pending), "Keep agent running");

                // teardown
                cancellationTokenSource.Cancel();
                await UniTask.NextFrame();
            }
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task Run_ExistDisabledEmergencyExitAnnotation_DoesNotClickEmergencyExitButton()
        {
            var agent = CreateAgent();

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var token = cancellationTokenSource.Token;
                var task = agent.Run(token);
                await UniTask.NextFrame();

                var emergencyButton = AttachEmergencyExitAnnotation(annotationEnabled: false);
                await UniTask.Delay(200);

                Assert.That(emergencyButton.IsClicked, Is.False, "Does not click button with EmergencyExit annotation");
                Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Pending), "Keep agent running");

                // teardown
                cancellationTokenSource.Cancel();
                await UniTask.NextFrame();
            }
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task Run_ExistNotInteractableEmergencyExitButton_DoesNotClickEmergencyExitButton()
        {
            var agent = CreateAgent();

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var token = cancellationTokenSource.Token;
                var task = agent.Run(token);
                await UniTask.NextFrame();

                var emergencyButton = AttachEmergencyExitAnnotation(buttonInteractable: false);
                await UniTask.Delay(200);

                Assert.That(emergencyButton.IsClicked, Is.False, "Does not click button with EmergencyExit annotation");
                Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Pending), "Keep agent running");

                // teardown
                cancellationTokenSource.Cancel();
                await UniTask.NextFrame();
            }
        }

        [Test]
        [LoadScene(TestScene)]
        public async Task Run_ExistNotReachableEmergencyExitButton_DoesNotClickEmergencyExitButton()
        {
            var agent = CreateAgent();

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var token = cancellationTokenSource.Token;
                var task = agent.Run(token);
                await UniTask.NextFrame();

                var emergencyButton = AttachEmergencyExitAnnotation(reachable: false);
                await UniTask.Delay(200);

                Assert.That(emergencyButton.IsClicked, Is.False, "Does not click button with EmergencyExit annotation");
                Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Pending), "Keep agent running");

                // teardown
                cancellationTokenSource.Cancel();
                await UniTask.NextFrame();
            }
        }

        [Test]
        [FocusGameView]
        [LoadScene(TestScene)]
        public async Task Run_EnableScreenshot_SaveScreenshotToFile()
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
            agent.screenshot = true;

            TwoTieredCounterStrategy.ResetPrefixCounters();

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var token = cancellationTokenSource.Token;
                var task = agent.Run(token);
                await UniTask.NextFrame();

                var emergencyButton = AttachEmergencyExitAnnotation();
                await UniTask.Delay(200);

                Assert.That(emergencyButton.IsClicked, Is.True, "Clicked button with EmergencyExit annotation");
                Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Pending), "Keep agent running");
                Assert.That(path, Does.Exist, "Save screenshot to file");

                // teardown
                cancellationTokenSource.Cancel();
                await UniTask.NextFrame();
            }

            AutopilotState.Instance.Reset();
        }
    }
}
