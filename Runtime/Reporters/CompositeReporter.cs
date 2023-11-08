// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Settings;
using UnityEngine;

namespace DeNA.Anjin.Reporters
{
    /// <summary>
    /// A class for reporters that delegate to multiple reporters
    /// </summary>
    [CreateAssetMenu(fileName = "New CompositeReporter", menuName = "Anjin/Composite Reporter", order = 51)]
    public class CompositeReporter : AbstractReporter
    {
        /// <summary>
        /// Reporters to delegate
        /// </summary>
        public List<AbstractReporter> reporters = new List<AbstractReporter>();


        /// <inheritdoc />
        public override async UniTask PostReportAsync(
            AutopilotSettings settings,
            string logString,
            string stackTrace,
            LogType type,
            bool withScreenshot,
            CancellationToken cancellationToken = default
        )
        {
            await UniTask.WhenAll(
                reporters
                    .Where(r => r != this && r != null)
                    .Select(
                        r => r.PostReportAsync(settings, logString, stackTrace, type, withScreenshot, cancellationToken)
                    )
            );
        }
    }
}
