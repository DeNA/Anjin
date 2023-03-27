// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Cysharp.Threading.Tasks;
using TestHelper.Monkey;
using TestHelper.Monkey.Random;
using UnityEngine;

namespace DeNA.Anjin.Agents
{
    /// <summary>
    /// This is an Agent that randomly manipulates uGUI components.
    /// </summary>
    [CreateAssetMenu(fileName = "New UGUIMonkeyAgent", menuName = "Anjin/uGUI Monkey Agent", order = 30)]
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase")]
    public class UGUIMonkeyAgent : AbstractAgent
    {
        /// <summary>
        /// Agent running lifespan [sec]. When specified zero, so unlimited running
        /// </summary>
        public long lifespanSec;

        /// <summary>
        /// Delay time between random operations [ms]
        /// </summary>
        public int delayMillis = 200;

        /// <inheritdoc />
        public override async UniTask Run(CancellationToken token)
        {
            Logger.Log($"Enter {this.name}.Run()");

            var config = new MonkeyConfig
            {
                Lifetime = lifespanSec > 0 ? TimeSpan.FromSeconds(lifespanSec) : TimeSpan.MaxValue,
                DelayMillis = delayMillis,
                Random = new RandomImpl(this.Random.Next()),
                Logger = this.Logger,
            };
            await Monkey.Run(config, token);

            Logger.Log($"Exit {this.name}.Run()");
        }
    }
}
