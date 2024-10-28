// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Collections;
using DeNA.Anjin.Settings;
using UnityEngine.TestTools;

namespace DeNA.Anjin.Editor.YieldInstructions
{
    /// <summary>
    /// Wait autopilot to terminate in Edit Mode tests.
    /// </summary>
    public class WaitForAutopilotToTerminate : IEditModeTestYieldInstruction
    {
        /// <inheritdoc/>
        public bool ExpectDomainReload => false;

        /// <inheritdoc/>
        public bool ExpectedPlaymodeState => false;

        /// <inheritdoc/>
        public IEnumerator Perform()
        {
            while (AutopilotState.Instance.IsRunning)
            {
                yield return null;
            }
        }
    }
}
