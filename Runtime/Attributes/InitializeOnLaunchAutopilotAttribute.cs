// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using UnityEngine.Scripting;

namespace DeNA.Anjin.Attributes
{
    /// <summary>
    /// Attach to a static method that performs game title-specific initialization at autopilot startup.
    /// Initialization is performed at the timing of `RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)`.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class InitializeOnLaunchAutopilotAttribute : PreserveAttribute
    {
    }
}
