// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DeNA.Anjin.Agents
{
    /// <summary>
    /// An Agent that will fail if a defuse message is not received before the inner agent exits.
    ///
    /// For example, pass the out-game tutorial (Things that can be completed with tap operations on uGUI, such as smartphone games).
    ///     1. Set `UGUIMonkeyAgent` as the `Agent`. It should not be able to operate except for the advancing button. Set the `Lifespan Sec` with a little margin.
    ///     2. Set the log message to be output when the tutorial is completed as the `Defuse Message`.
    /// </summary>
    [CreateAssetMenu(fileName = "New TimeBombAgent", menuName = "Anjin/Time Bomb Agent", order = 17)]
    public class TimeBombAgent : AbstractAgent
    {
        /// <summary>
        /// Working Agent. If this Agent exits first, the TimeBombAgent will fail.
        /// </summary>
        public AbstractAgent agent;

        /// <summary>
        /// Defuse the time bomb when this message comes to the log first.
        /// Can specify regex.
        /// </summary>
        public string defuseMessage;

        private Regex DefuseMessageRegex => new Regex(defuseMessage);
        private CancellationTokenSource _cts;

        private void OnEnable()
        {
            Application.logMessageReceived += HandleDefuseMessage;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleDefuseMessage;
        }

        private void HandleDefuseMessage(string logString, string stackTrace, LogType type)
        {
            if (DefuseMessageRegex.IsMatch(logString))
            {
                try
                {
                    _cts?.Cancel();
                }
                catch (ObjectDisposedException e)
                {
                    // ignored
                }
            }
        }

        /// <inheritdoc />
        public override async UniTask Run(CancellationToken token)
        {
            Logger.Log($"Enter {this.name}.Run()");

            try
            {
                using (var agentCts = new CancellationTokenSource()) // To cancel only the Working Agent.
                {
                    _cts = agentCts;

                    using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, agentCts.Token))
                    {
                        try
                        {
                            agent.Logger = Logger;
                            agent.Random = Random;
                            // Note: This Agent does not consume pseudo-random numbers, so passed on as is.
                            await agent.Run(linkedCts.Token);

                            throw new TimeoutException(
                                $"Could not receive defuse message `{defuseMessage}` before the agent terminated.");
                        }
                        catch (OperationCanceledException e)
                        {
                            if (token.IsCancellationRequested) // The parent was cancelled.
                            {
                                throw;
                            }

                            Logger.Log($"Working agent {agent.name} was cancelled.");
                        }
                    }
                }
            }
            finally
            {
                Logger.Log($"Exit {this.name}.Run()");
            }
        }
    }
}
