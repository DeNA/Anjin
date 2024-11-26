// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using UnityEngine;

namespace DeNA.Anjin.ArgumentCapture
{
    internal static class ArgumentCaptureIos
    {
        /// <summary>
        /// For iOS player
        /// </summary>
        /// <remarks>
        /// In Xcode, set "Arguments Passed On Launch" in Edit Scheme | Run | "Arguments" tab
        /// </remarks>
        /// <param name="key"></param>
        /// <returns></returns>
        internal static string CaptureString(string key)
        {
            if (PlayerPrefs.HasKey(key))
            {
                return PlayerPrefs.GetString(key);
            }

            return null;
        }
    }
}
