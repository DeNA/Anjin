// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using UnityEngine.Scripting;

namespace DeNA.Anjin.Attributes
{
    /// <summary>
    /// Attach to a static method that performs game title-specific initialization at autopilot startup.
    /// Called when `RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)`.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class InitializeOnLaunchAutopilotAttribute : PreserveAttribute
    {
        internal const int InitializeLoggerOrder = -20000;
        internal const int InitializeSettings = -10000;
        internal const int DefaultOrder = 0;

        /// <summary>
        /// Relative callback order for callbacks. Callbacks with lower values are called before ones with higher values.
        /// </summary>
        public int CallbackOrder { get; private set; }

        /// <summary>
        /// Attach to a static method that performs game title-specific initialization at autopilot startup.
        /// Called when `RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)`.
        /// </summary>
        /// <param name="callbackOrder">Relative callback order. Callbacks with lower values are called before ones with higher values.</param>
        public InitializeOnLaunchAutopilotAttribute(int callbackOrder = DefaultOrder)
        {
            this.CallbackOrder = callbackOrder;
        }
    }
}
