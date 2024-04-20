// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DeNA.Anjin.Agents
{
    /// <summary>
    /// An Agent that executes one child Agent indefinitely and repeatedly.
    /// </summary>
    [CreateAssetMenu(fileName = "New RepeatAgent", menuName = "Anjin/Repeat Agent", order = 15)]
    public class RepeatAgent : AbstractAgent
    {
        /// <summary>
        /// Agent to repeat execution
        /// </summary>
        public AbstractAgent agent;

        /// <inheritdoc />
        [SuppressMessage("Blocker Bug", "S2190:Recursion should not be infinite")]
        [SuppressMessage("ReSharper", "FunctionNeverReturns")]
        public override async UniTask Run(CancellationToken token)
        {
            Logger.Log($"Enter {this.name}.Run()");

            agent.Logger = Logger;
            agent.Random = Random; // This Agent does not consume pseudo-random numbers, so passed on as is.
            try
            {
                while (true) // Note: This agent is not terminate myself
                {
                    await agent.Run(token);
                }
            }
            finally
            {
                Logger.Log($"Exit {this.name}.Run()");
            }
        }
    }
}
