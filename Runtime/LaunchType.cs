// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

namespace DeNA.Anjin
{
    /// <summary>
    /// Define of what autopilot was launched by
    /// </summary>
    public enum LaunchType
    {
        /// <summary>
        /// Not launch yet
        /// </summary>
        NotSet = 0,

        /// <summary>
        /// Launch via Edit mode
        /// </summary>
        EditorEditMode,

        /// <summary>
        /// Launch via Play mode
        /// </summary>
        EditorPlayMode,

        /// <summary>
        /// Launch from commandline interface
        /// When autopilot is finished, Unity editor is also exit.
        /// </summary>
        Commandline,

        /// <summary>
        /// Launch on standalone platform player build (not support yet)
        /// </summary>
        Runtime,
    }
}
