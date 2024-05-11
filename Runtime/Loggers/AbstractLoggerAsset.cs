// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using UnityEngine;

namespace DeNA.Anjin.Loggers
{
    /// <summary>
    /// Abstract logger settings used for autopilot.
    /// </summary>
    public abstract class AbstractLoggerAsset : ScriptableObject, IDisposable
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
        public abstract ILogger Logger { get; }

        /// <inheritdoc />
        public abstract void Dispose();
    }
}
