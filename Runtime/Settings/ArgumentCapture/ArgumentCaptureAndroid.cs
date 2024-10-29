// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using UnityEngine;

namespace DeNA.Anjin.Settings.ArgumentCapture
{
    internal static class ArgumentCaptureAndroid
    {
        /// <summary>
        /// For Android player
        /// </summary>
        /// <remarks>
        /// e.g., `${ADB} shell "am start -n com.examples.package_name/com.unity3d.player.UnityPlayerActivity -e EXTRA1 VALUE1 -e EXTRA2 VALUE2 --ei EXTRA_INT 128 --ez EXTRA_BOOL true"`
        /// Numeric type should also be passed as `-e` (string), since Extra has a type, values passed as `-ez`, `-ei`, etc. will be missing in `getStringExtra()`.
        /// </remarks>
        /// <param name="key"></param>
        /// <returns></returns>
        internal static string CaptureString(string key)
        {
            using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var intent = activity.Call<AndroidJavaObject>("getIntent"))
            {
                return intent.Call<string>("getStringExtra", key);
            }
        }
    }
}
