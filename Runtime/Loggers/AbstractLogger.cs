// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using UnityEngine;

namespace DeNA.Anjin.Loggers
{
    /// <summary>
    /// Abstract logger settings used for autopilot.
    /// </summary>
    public abstract class AbstractLogger : ScriptableObject
    {
#if UNITY_EDITOR
        /// <summary>
        /// Description about this agent instance.
        /// </summary>
        [Multiline] public string description;
#endif

        /// <summary>
        /// Logger implementation used for autopilot.
        /// </summary>
        public ILogger LoggerImpl { get; protected set; }
    }
}
