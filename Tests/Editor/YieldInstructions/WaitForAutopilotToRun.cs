// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Collections;
using DeNA.Anjin.Settings;
using UnityEditor;
using UnityEngine.TestTools;

namespace DeNA.Anjin.Editor.YieldInstructions
{
    /// <summary>
    /// Wait autopilot to run in Edit Mode tests.
    /// </summary>
    public class WaitForAutopilotToRun : IEditModeTestYieldInstruction
    {
        /// <inheritdoc/>
        public bool ExpectDomainReload => true;

        /// <inheritdoc/>
        public bool ExpectedPlaymodeState => true;

        /// <inheritdoc/>
        public IEnumerator Perform()
        {
            while (!AutopilotState.Instance.IsRunning && !EditorApplication.isPlaying)
            {
                yield return null;
            }
        }
    }
}
