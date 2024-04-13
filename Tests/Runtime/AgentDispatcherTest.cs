// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using DeNA.Anjin.Agents;
using DeNA.Anjin.Settings;
using DeNA.Anjin.Utilities;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DeNA.Anjin
{
    [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.WindowsEditor, RuntimePlatform.LinuxEditor)]
    [SuppressMessage("ApiDesign", "RS0030")]
    public class AgentDispatcherTest
    {
        [SetUp]
        public void SetUp()
        {
            Assume.That(GameObject.Find(nameof(DoNothingAgent)), Is.Null);
        }

        [TearDown]
        public async Task TearDown()
        {
            var testAgentObject = GameObject.Find(nameof(DoNothingAgent));
            Object.Destroy(testAgentObject);
            await Task.Delay(100);
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

        private const string TestScenePath = "Packages/com.dena.anjin/Tests/TestScenes/Buttons.unity";

        private static Scene LoadTestScene()
        {
            return EditorSceneManager.LoadSceneInPlayMode(
                TestScenePath,
                new LoadSceneParameters(LoadSceneMode.Single));
        }

        [Test]
        public void DispatchByScene_DispatchAgentBySceneAgentMaps()
        {
            const string ActualAgentName = nameof(DoNothingAgent);

            var settings = CreateAutopilotSettings();
            settings.sceneAgentMaps.Add(new SceneAgentMap
            {
                scenePath = TestScenePath, agent = CreateDoNothingAgent()
            });

            var logger = new ConsoleLogger(Debug.unityLogger.logHandler);
            var randomFactory = new RandomFactory(0);
            var dispatcher = new AgentDispatcher(settings, logger, randomFactory);
            dispatcher.DispatchByScene(LoadTestScene());

            var gameObject = GameObject.Find(ActualAgentName);
            Assert.That(gameObject, Is.Not.Null);
        }

        [Test]
        public void DispatchByScene_DispatchFallbackAgent()
        {
            const string ActualAgentName = "Fallback";

            var settings = CreateAutopilotSettings();
            settings.fallbackAgent = CreateDoNothingAgent(ActualAgentName);

            var logger = new ConsoleLogger(Debug.unityLogger.logHandler);
            var randomFactory = new RandomFactory(0);
            var dispatcher = new AgentDispatcher(settings, logger, randomFactory);
            dispatcher.DispatchByScene(LoadTestScene());

            var gameObject = GameObject.Find(ActualAgentName);
            Assert.That(gameObject, Is.Not.Null);
        }

        [Test]
        public void DispatchByScene_NoSceneAgentMapsAndFallbackAgent_AgentIsNotDispatch()
        {
            var settings = CreateAutopilotSettings();
            var logger = new ConsoleLogger(Debug.unityLogger.logHandler);
            var randomFactory = new RandomFactory(0);
            var dispatcher = new AgentDispatcher(settings, logger, randomFactory);
            dispatcher.DispatchByScene(LoadTestScene());

            LogAssert.Expect(LogType.Warning, "Agent not found by scene: Buttons");
        }

        [Test]
        public void DispatchByScene_DispatchObserverAgent()
        {
            const string ActualAgentName = "Observer";

            var settings = CreateAutopilotSettings();
            settings.fallbackAgent = CreateDoNothingAgent();
            settings.observerAgent = CreateDoNothingAgent(ActualAgentName);

            var logger = new ConsoleLogger(Debug.unityLogger.logHandler);
            var randomFactory = new RandomFactory(0);
            var dispatcher = new AgentDispatcher(settings, logger, randomFactory);
            dispatcher.DispatchByScene(LoadTestScene());

            var gameObject = GameObject.Find(ActualAgentName);
            Assert.That(gameObject, Is.Not.Null);
        }
    }
}
