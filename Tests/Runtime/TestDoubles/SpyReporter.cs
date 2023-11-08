// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Reporters;
using DeNA.Anjin.Settings;
using UnityEngine;

namespace DeNA.Anjin.TestDoubles
{
    /// <summary>
    /// A spy for <c cref="AbstractReporter" />
    /// </summary>
    // [CreateAssetMenu(fileName = "New SpyReporter", menuName = "Anjin/Spy Reporter", order = 34)]
    public class SpyReporter : AbstractReporter
    {
        public List<Dictionary<string, string>> Arguments { get; } = new List<Dictionary<string, string>>();
        
        public override async UniTask PostReportAsync(
            AutopilotSettings settings,
            string logString,
            string stackTrace,
            LogType type,
            bool withScreenshot,
            CancellationToken cancellationToken = default
        )
        {
            Debug.Log("Reporter called");
            Arguments.Add(new Dictionary<string, string>
            {
                {"settings", settings.ToString()},
                {"logString", logString},
                {"stackTrace", stackTrace},
                {"type", type.ToString()},
                {"withScreenshot", withScreenshot.ToString()}
            });
            await UniTask.NextFrame(cancellationToken);
        }
    }
}
