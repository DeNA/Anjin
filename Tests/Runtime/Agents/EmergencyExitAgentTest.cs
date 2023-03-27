// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Annotations;
using DeNA.Anjin.TestDoubles;
using DeNA.Anjin.Utilities;
using NUnit.Framework;
using UnityEngine;
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
            agent.Logger = new ConsoleLogger(Debug.unityLogger.logHandler);
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = nameof(Run_cancelTask_stopAgent);

            var agentName = agent.GetType().Name;
            var gameObject = new GameObject(agentName);
            var token = gameObject.GetCancellationTokenOnDestroy();
            var task = agent.Run(token);
            await UniTask.Delay(TimeSpan.FromMilliseconds(500), cancellationToken: token);

            Object.DestroyImmediate(gameObject);
            // ReSharper disable once MethodSupportsCancellation
            await UniTask.NextFrame();

            Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Canceled));
        }

        private static SpyButton CreateButtonWithEmergencyExitAnnotation()
        {
            var button = new GameObject();
            button.AddComponent<EmergencyExit>();
            return button.AddComponent<SpyButton>();
        }

        [Test]
        public async Task Run_existEmergencyExitButton_ClickEmergencyExitButton()
        {
            var agent = ScriptableObject.CreateInstance<EmergencyExitAgent>();
            agent.Logger = new ConsoleLogger(Debug.unityLogger.logHandler);
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = nameof(Run_cancelTask_stopAgent);

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var token = cancellationTokenSource.Token;
                var task = agent.Run(token);
                await UniTask.Delay(TimeSpan.FromMilliseconds(500), cancellationToken: token);

                var emergencyButton = CreateButtonWithEmergencyExitAnnotation();
                await UniTask.NextFrame(token);

                Assert.That(emergencyButton.IsClicked, Is.True, "Clicked button with EmergencyExit annotation");
                Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Pending), "Keep agent running");
            }
        }

        [Test]
        public async Task Run_existNotInteractableEmergencyExitButton_DoesNotClickEmergencyExitButton()
        {
            var agent = ScriptableObject.CreateInstance<EmergencyExitAgent>();
            agent.Logger = new ConsoleLogger(Debug.unityLogger.logHandler);
            agent.Random = new RandomFactory(0).CreateRandom();
            agent.name = nameof(Run_cancelTask_stopAgent);

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                var token = cancellationTokenSource.Token;
                var task = agent.Run(token);
                await UniTask.Delay(TimeSpan.FromMilliseconds(500), cancellationToken: token);

                var emergencyButton = CreateButtonWithEmergencyExitAnnotation();
                emergencyButton.gameObject.GetComponent<Button>().interactable = false;
                await UniTask.NextFrame(token);

                Assert.That(emergencyButton.IsClicked, Is.False, "Does not click button with EmergencyExit annotation");
                Assert.That(task.Status, Is.EqualTo(UniTaskStatus.Pending), "Keep agent running");
            }
        }
    }
}
