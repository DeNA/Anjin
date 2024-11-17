// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Annotations;
using DeNA.Anjin.TestDoubles;
using DeNA.Anjin.Utilities;
using NUnit.Framework;
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
            agent.Random = new RandomFactory(0).CreateRandom();
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
        public async Task Run_ExistEmergencyExitButton_ClickEmergencyExitButton()
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
    }
}
