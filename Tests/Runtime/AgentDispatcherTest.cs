// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using DeNA.Anjin.Agents;
using DeNA.Anjin.Settings;
using DeNA.Anjin.Utilities;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

#pragma warning disable CS0618 // Type or member is obsolete

namespace DeNA.Anjin
{
    [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor, RuntimePlatform.LinuxEditor)]
    [SuppressMessage("ApiDesign", "RS0030")]
    public class AgentDispatcherTest
    {
        private IAgentDispatcher _dispatcher;

        [TearDown]
        public void TearDown()
        {
            SceneManager.activeSceneChanged -= _dispatcher.DispatchByScene;
        }

        private static AutopilotSettings CreateAutopilotSettings()
        {
            var testSettings = ScriptableObject.CreateInstance<AutopilotSettings>();
            testSettings.sceneAgentMaps = new List<SceneAgentMap>();
            return testSettings;
        }

        private static DoNothingAgent CreateDoNothingAgent(string name = nameof(DoNothingAgent))
        {
            var doNothingAgent = ScriptableObject.CreateInstance<DoNothingAgent>();
            doNothingAgent.name = name;
            return doNothingAgent;
        }

        private void SetUpDispatcher(AutopilotSettings settings)
        {
            var logger = new ConsoleLogger(Debug.unityLogger.logHandler);
            var randomFactory = new RandomFactory(0);

            _dispatcher = new AgentDispatcher(settings, logger, randomFactory);
            SceneManager.activeSceneChanged += _dispatcher.DispatchByScene;
        }

        private const string TestScenePath = "Packages/com.dena.anjin/Tests/TestScenes/Buttons.unity";

        private static async Task LoadTestSceneAsync(string path)
        {
#if UNITY_EDITOR
            await EditorSceneManager.LoadSceneAsyncInPlayMode(path,
                new LoadSceneParameters(LoadSceneMode.Single));
#endif
        }

        [Test]
        public async Task DispatchByScene_DispatchAgentBySceneAgentMaps()
        {
            const string AgentName = "Mapped Agent";
            var settings = CreateAutopilotSettings();
            settings.sceneAgentMaps.Add(new SceneAgentMap
            {
                scenePath = TestScenePath, agent = CreateDoNothingAgent(AgentName)
            });
            SetUpDispatcher(settings);

            await LoadTestSceneAsync(TestScenePath);

            var actual = Object.FindObjectsOfType<AsyncDestroyTrigger>().Select(x => x.name);
            Assert.That(actual, Is.EquivalentTo(new[] { AgentName }));
        }

        [Test]
        public async Task DispatchByScene_DispatchFallbackAgent()
        {
            const string AgentName = "Fallback Agent";
            var settings = CreateAutopilotSettings();
            settings.fallbackAgent = CreateDoNothingAgent(AgentName);
            SetUpDispatcher(settings);

            await LoadTestSceneAsync(TestScenePath);

            var actual = Object.FindObjectsOfType<AsyncDestroyTrigger>().Select(x => x.name);
            Assert.That(actual, Is.EquivalentTo(new[] { AgentName }));
        }

        [Test]
        public async Task DispatchByScene_NoSceneAgentMapsAndFallbackAgent_AgentIsNotDispatch()
        {
            var settings = CreateAutopilotSettings();
            SetUpDispatcher(settings);

            await LoadTestSceneAsync(TestScenePath);

            var actual = Object.FindObjectsOfType<AsyncDestroyTrigger>().Select(x => x.name);
            Assert.That(actual, Is.Empty);
            LogAssert.Expect(LogType.Warning, "Agent not found by scene: Buttons");
        }

        [Test]
        public async Task DispatchByScene_DispatchObserverAgent()
        {
            const string AgentName = "Observer Agent";
            var settings = CreateAutopilotSettings();
            settings.observerAgent = CreateDoNothingAgent(AgentName);
            SetUpDispatcher(settings);

            await LoadTestSceneAsync(TestScenePath);

            var actual = Object.FindObjectsOfType<AsyncDestroyTrigger>().Select(x => x.name);
            Assert.That(actual, Is.EquivalentTo(new[] { AgentName }));
        }
    }
}
