// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Annotations;
using DeNA.Anjin.Settings;
using DeNA.Anjin.TestDoubles;
using DeNA.Anjin.Utilities;
using NUnit.Framework;
using TestHelper.Attributes;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace DeNA.Anjin.Agents
{
    [SuppressMessage("ReSharper", "ConvertToUsingDeclaration")]
    [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
    public class UGUIEmergencyExitAgentTest
    {
        [Test]
        public async Task Run_CancelTask_StopAgent()
        {
            var agent = ScriptableObject.CreateInstance<UGUIEmergencyExitAgent>();
            agent.Logger = Debug.unityLogger;
            agent.name = TestContext.CurrentContext.Test.Name;

            var gameObject = new GameObject();
            var token = gameObject.GetCancellationTokenOnDestroy();
            var task = agent.Run(token);
            await UniTask.NextFrame();

            Object.DestroyImmediate(gameObject);
            await UniTask.NextFrame();

            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Canceled));

            LogAssert.Expect(LogType.Log, $"Enter {agent.name}.Run()");
            LogAssert.Expect(LogType.Log, $"Exit {agent.name}.Run()");
        }

        private static SpyButton CreateButtonWithEmergencyExitAnnotation(bool interactable = true)
        {
            var button = new GameObject().AddComponent<SpyButton>();
            button.GetComponent<Selectable>().interactable = interactable;
            button.gameObject.AddComponent<EmergencyExitAnnotation>();
            return button;
        }

        [Test]
        [CreateScene]
        public async Task Run_ExistEmergencyExitButton_ClickEmergencyExitButton()
        {
            var agent = ScriptableObject.CreateInstance<UGUIEmergencyExitAgent>();
            agent.Logger = Debug.unityLogger;
            agent.name = TestContext.CurrentContext.Test.Name;
            agent.intervalMillis = 100;

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var token = cancellationTokenSource.Token;
                var task = agent.Run(token);
                await UniTask.NextFrame();

                var emergencyButton = CreateButtonWithEmergencyExitAnnotation();
                await UniTask.Delay(200);

                Assert.That(emergencyButton.IsClicked, Is.True, "Clicked button with EmergencyExit annotation");
                Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Pending), "Keep agent running");

                cancellationTokenSource.Cancel();
                await UniTask.NextFrame();
            }

            LogAssert.Expect(LogType.Log, $"Enter {agent.name}.Run()");
            LogAssert.Expect(LogType.Log, $"Exit {agent.name}.Run()");
        }

        [Test]
        [CreateScene]
        public async Task Run_ExistNotInteractableEmergencyExitButton_DoesNotClickEmergencyExitButton()
        {
            var agent = ScriptableObject.CreateInstance<UGUIEmergencyExitAgent>();
            agent.Logger = Debug.unityLogger;
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = TestContext.CurrentContext.Test.Name;
            agent.intervalMillis = 100;

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var token = cancellationTokenSource.Token;
                var task = agent.Run(token);
                await UniTask.NextFrame();

                var emergencyButton = CreateButtonWithEmergencyExitAnnotation(false);
                await UniTask.Delay(200);

                Assert.That(emergencyButton.IsClicked, Is.False, "Does not click button with EmergencyExit annotation");
                Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Pending), "Keep agent running");

                cancellationTokenSource.Cancel();
                await UniTask.NextFrame();
            }

            LogAssert.Expect(LogType.Log, $"Enter {agent.name}.Run()");
            LogAssert.Expect(LogType.Log, $"Exit {agent.name}.Run()");
        }

        [Test]
        [FocusGameView]
        [CreateScene]
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

            var agent = ScriptableObject.CreateInstance<UGUIEmergencyExitAgent>();
            agent.Logger = Debug.unityLogger;
            agent.name = agentName;
            agent.intervalMillis = 100;
            agent.screenshot = true;

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var token = cancellationTokenSource.Token;
                var task = agent.Run(token);
                await UniTask.NextFrame();

                var emergencyButton = CreateButtonWithEmergencyExitAnnotation();
                await UniTask.Delay(200);

                Assert.That(emergencyButton.IsClicked, Is.True, "Clicked button with EmergencyExit annotation");
                Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Pending), "Keep agent running");
                Assert.That(path, Does.Exist, "Save screenshot to file");

                // teardown
                cancellationTokenSource.Cancel();
                await UniTask.NextFrame();
                AutopilotState.Instance.Reset();
            }
        }
    }
}
