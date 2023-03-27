// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using DeNA.Anjin.Agents;
using DeNA.Anjin.Settings;
using DeNA.Anjin.Utilities;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DeNA.Anjin
{
    /// <summary>
    /// Agent dispatcher interface
    /// </summary>
    public interface IAgentDispatcher
    {
        /// <summary>
        /// Agent dispatch by next scene
        /// </summary>
        /// <param name="current">Current scene</param>
        /// <param name="next">Next transition scene</param>
        void DispatchByScene(Scene current, Scene next);

        /// <summary>
        /// Agent dispatch by current scene
        /// </summary>
        /// <param name="scene">Current scene</param>
        void DispatchByScene(Scene scene);
    }

    /// <inheritdoc />
    public class AgentDispatcher : IAgentDispatcher
    {
        private readonly AutopilotSettings _settings;
        private readonly ILogger _logger;
        private readonly RandomFactory _randomFactory;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="logger"></param>
        /// <param name="randomFactory"></param>
        public AgentDispatcher(AutopilotSettings settings, ILogger logger, RandomFactory randomFactory)
        {
            _settings = settings;
            _logger = logger;
            _randomFactory = randomFactory;
        }

        /// <summary>
        /// Dispatch agent mapped to Scene `next`
        /// </summary>
        /// <param name="current"></param>
        /// <param name="next"></param>
        public void DispatchByScene(Scene current, Scene next)
        {
            DispatchByScene(next);
        }

        /// <summary>
        /// Dispatch agent mapped to Scene
        /// </summary>
        /// <param name="scene"></param>
        public void DispatchByScene(Scene scene)
        {
            AbstractAgent agent = null;

            foreach (var sceneAgentMap in _settings.sceneAgentMaps)
            {
                if (sceneAgentMap.scenePath.Equals(scene.path))
                {
                    agent = sceneAgentMap.agent;
                    break;
                }
            }

            if (!agent)
            {
                if (!_settings.fallbackAgent)
                {
                    _logger.Log(LogType.Warning, $"Agent not found by scene: {scene.name}");
                    return;
                }

                _logger.Log($"Use fallback agent. scene: {scene.path}");
                agent = _settings.fallbackAgent;
            }

            DispatchAgent(agent);

            if (_settings.observerAgent != null)
            {
                DispatchAgent(_settings.observerAgent);
                // Note: The ObserverAgent is not made to DontDestroyOnLoad, to start every time.
                //      Because it is a source of bugs to force the implementation of DontDestroyOnLoad to the descendants of the Composite.
            }
        }

        private void DispatchAgent(AbstractAgent agent)
        {
            var agentName = agent.name;
            var gameObject = new GameObject(agentName);
            var token = gameObject.GetCancellationTokenOnDestroy(); // Agent also dies when GameObject is destroyed

            agent.Logger = _logger;
            agent.Random = _randomFactory.CreateRandom();
            agent.Run(token).Forget();

            _logger.Log($"Agent {agentName} dispatched!");
        }
    }
}
