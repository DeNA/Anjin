// Copyright (c) 2023 DeNA Co., Ltd.
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
    public class EmergencyExitAgentTest
    {
        [Test]
        public async Task Run_cancelTask_stopAgent()
        {
            var agent = ScriptableObject.CreateInstance<EmergencyExitAgent>();
            agent.Logger = Debug.unityLogger;
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = nameof(Run_cancelTask_stopAgent);

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
        public async Task Run_existEmergencyExitButton_ClickEmergencyExitButton()
        {
            var agent = ScriptableObject.CreateInstance<EmergencyExitAgent>();
            agent.Logger = Debug.unityLogger;
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = nameof(Run_cancelTask_stopAgent);

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var token = cancellationTokenSource.Token;
                var task = agent.Run(token);
                await UniTask.NextFrame();

                var emergencyButton = CreateButtonWithEmergencyExitAnnotation();
                await UniTask.NextFrame(token);

                Assert.That(emergencyButton.IsClicked, Is.True, "Clicked button with EmergencyExit annotation");
                Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Pending), "Keep agent running");

                cancellationTokenSource.Cancel();
                await UniTask.NextFrame();
            }

            LogAssert.Expect(LogType.Log, $"Enter {agent.name}.Run()");
            LogAssert.Expect(LogType.Log, $"Exit {agent.name}.Run()");
        }

        [Test]
        public async Task Run_existNotInteractableEmergencyExitButton_DoesNotClickEmergencyExitButton()
        {
            var agent = ScriptableObject.CreateInstance<EmergencyExitAgent>();
            agent.Logger = Debug.unityLogger;
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = nameof(Run_cancelTask_stopAgent);

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var token = cancellationTokenSource.Token;
                var task = agent.Run(token);
                await UniTask.NextFrame();

                var emergencyButton = CreateButtonWithEmergencyExitAnnotation(false);
                await UniTask.NextFrame(token);

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
