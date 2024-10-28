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
        /// When autopilot is finished, exits play mode.
        /// </summary>
        EditMode,

        /// <summary>
        /// Launch via Play mode (includes play mode tests and player build)
        /// </summary>
        PlayMode,

        /// <summary>
        /// Launch from commandline interface (Editor or Player)
        /// When autopilot is finished, Unity editor/player is also exit.
        /// </summary>
        Commandline,
    }
}
