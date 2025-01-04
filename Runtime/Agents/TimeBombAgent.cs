// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Attributes;
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

#pragma warning disable IDISP006
        private CancellationTokenSource _agentCts; // dispose in using block
#pragma warning restore IDISP006
        private Regex _defuseMessageRegex;

        private Regex DefuseMessageRegex
        {
            get
            {
                if (_defuseMessageRegex == null)
                {
                    _defuseMessageRegex = new Regex(defuseMessage);
                }

                return _defuseMessageRegex;
            }
            set { _defuseMessageRegex = value; }
        }

        [InitializeOnLaunchAutopilot]
        private static void ResetInstances()
        {
            // Reset runtime instances
            var agents = Resources.FindObjectsOfTypeAll<TimeBombAgent>();
            foreach (var agent in agents)
            {
                agent._agentCts?.Dispose();
                agent._agentCts = null;
                agent.DefuseMessageRegex = null;
            }
        }

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
                    _agentCts?.Cancel();
                }
                catch (ObjectDisposedException)
                {
                    // ignored
                }
            }
        }

        /// <inheritdoc />
        public override async UniTask Run(CancellationToken cancellationToken)
        {
            Logger.Log($"Enter {this.name}.Run()");

            try
            {
                using (_agentCts = new CancellationTokenSource()) // To cancel only the Working Agent.
                {
                    using (var linkedCts =
                           CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _agentCts.Token))
                    {
                        try
                        {
                            agent.Logger = Logger;
                            agent.Random = Random;
                            // Note: This Agent does not consume pseudo-random numbers, so passed on as is.

                            await agent.Run(linkedCts.Token);

                            // If the working Agent exits first, the TimeBombAgent will fail.
                            var message =
                                $"Could not receive defuse message `{defuseMessage}` before the {agent.name} terminated.";
                            Logger.Log(message);

                            // ReSharper disable once MethodSupportsCancellation; Do not use this Agent's CancellationToken.
                            AutopilotInstance.TerminateAsync(ExitCode.AutopilotFailed, message,
                                new StackTrace(true).ToString()).Forget();
                        }
                        catch (OperationCanceledException)
                        {
                            if (cancellationToken.IsCancellationRequested) // The parent was cancelled.
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
