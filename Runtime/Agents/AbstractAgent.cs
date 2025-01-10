// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Threading;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Utilities;
using UnityEngine;

namespace DeNA.Anjin.Agents
{
    /// <summary>
    /// Abstract Agent
    /// </summary>
    public abstract class AbstractAgent : ScriptableObject
    {
#if UNITY_EDITOR
        /// <summary>
        /// Description about this agent instance
        /// </summary>
        [Multiline]
        public string description;
#endif
        /// <summary>
        /// Logger instance used by this agent
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Random instance used by this agent
        /// </summary>
        public virtual IRandom Random { get; set; }

        private ITerminatable _autopilotInstance;

        /// <summary>
        /// Running Autopilot instance.
        /// Can inject a spy object for testing.
        /// </summary>
        public ITerminatable AutopilotInstance
        {
            get
            {
                return _autopilotInstance ?? Autopilot.Instance;
            }
            set
            {
                _autopilotInstance = value;
            }
        }

        /// <summary>
        /// Run agent
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract UniTask Run(CancellationToken cancellationToken);
    }
}
