// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DeNA.Anjin.Agents
{
    /// <summary>
    /// Do nothing agent
    /// </summary>
    [CreateAssetMenu(fileName = "New DoNothingAgent", menuName = "Anjin/Do Nothing Agent", order = 20)]
    public class DoNothingAgent : AbstractAgent
    {
        /// <summary>
        /// Agent running lifespan [sec]. When specified zero, so unlimited running
        /// </summary>
        public long lifespanSec;

        /// <inheritdoc />
        public override async UniTask Run(CancellationToken token)
        {
            Logger.Log($"Enter {this.name}.Run()");

            try
            {
                if (lifespanSec > 0)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(lifespanSec), ignoreTimeScale: true,
                        cancellationToken: token);
                }
                else
                {
                    await UniTask.WaitWhile(() => true, cancellationToken: token); // Wait indefinitely
                }
            }
            finally
            {
                Logger.Log($"Exit {this.name}.Run()");
            }
        }
    }
}
