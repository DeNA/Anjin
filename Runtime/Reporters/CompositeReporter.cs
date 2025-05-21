// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DeNA.Anjin.Reporters
{
    /// <summary>
    /// A class for reporters that delegate to multiple reporters.
    /// This class is for Internal use. However, It is public for historical reasons.
    /// </summary>
    public class CompositeReporter : AbstractReporter
    {
        /// <summary>
        /// Reporters to delegate.
        /// </summary>
        public List<AbstractReporter> reporters = new List<AbstractReporter>();

        /// <inheritdoc />
        public override async UniTask PostReportAsync(
            string message,
            string stackTrace,
            ExitCode exitCode,
            CancellationToken cancellationToken = default
        )
        {
            await UniTask.WhenAll(
                reporters
                    .Where(r => r != this && r != null)
                    .Select(r => r.PostReportAsync(message, stackTrace, exitCode, cancellationToken)
                    )
            );
        }
    }
}
