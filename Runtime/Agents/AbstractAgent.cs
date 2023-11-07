// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Threading;
using DeNA.Anjin.Utilities;
using Cysharp.Threading.Tasks;
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

        /// <summary>
        /// Run agent
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public abstract UniTask Run(CancellationToken token);
    }
}
