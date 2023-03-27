// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using UnityEngine;

namespace DeNA.Anjin.ArgumentCapture
{
    internal static class ArgumentCapture
    {
        internal static (bool, T) Capture<T>(string key)
        {
            string captureString = null;
#if UNITY_EDITOR
            captureString = CaptureString(key);
#elif UNITY_ANDROID
            captureString = ArgumentCaptureAndroid.CaptureString(key);
#elif UNITY_IOS
            captureString = ArgumentCaptureIos.CaptureString(key);
#endif

            if (string.IsNullOrEmpty(captureString))
            {
                Debug.Log($"Argument not found. key: {key}");
                return (false, default(T));
            }

            switch (typeof(T))
            {
                case Type stringType when stringType == typeof(string):
                    return (true, (T)(object)captureString);
                case Type boolType when boolType == typeof(bool):
                    return (true, (T)(object)ParseBool(captureString));
                case Type intType when intType == typeof(int):
                    return (true, (T)(object)ParseInt(captureString));
                case Type longType when longType == typeof(long):
                    return (true, (T)(object)ParseLong(captureString));
                case Type floatType when floatType == typeof(float):
                    return (true, (T)(object)ParseFloat(captureString));
                case Type doubleType when doubleType == typeof(double):
                    return (true, (T)(object)ParseDouble(captureString));
            }

            Debug.LogError($"Unsupported type: {typeof(T).FullName}");
            return (false, default(T));
        }

        private static string CaptureString(string key)
        {
            // Arguments first
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i].ToLower().Equals($"-{key.ToLower()}"))
                {
                    if (i == args.Length - 1)
                    {
                        return string.Empty; // In Bool, it's possible that the termination is KEY.
                    }

                    return args[i + 1];
                }
            }

            // Check environment variables if not found in arguments
            var env = Environment.GetEnvironmentVariable(key);
            if (!string.IsNullOrEmpty(env))
            {
                return env;
            }

            return null;
        }

        private static bool ParseBool(string s)
        {
            if (string.IsNullOrEmpty(s) || s.StartsWith("-"))
            {
                return true; // In Bool and terminator is KEY (omitted VALUE), assumed to be true.
            }

            if (bool.TryParse(s, out var value))
            {
                return value;
            }

            Debug.LogWarning($"Argument captured, but not bool format. value: {s}");
            return default(bool);
        }

        private static int ParseInt(string s)
        {
            if (int.TryParse(s, out var value))
            {
                return value;
            }

            Debug.LogWarning($"Argument captured, but not number format. value: {s}");
            return default(int);
        }

        private static long ParseLong(string s)
        {
            if (long.TryParse(s, out var value))
            {
                return value;
            }

            Debug.LogWarning($"Argument captured, but not number format. value: {s}");
            return default(long);
        }

        private static float ParseFloat(string s)
        {
            if (float.TryParse(s, out var value))
            {
                return value;
            }

            Debug.LogWarning($"Argument captured, but not number format. value: {s}");
            return default(float);
        }

        private static double ParseDouble(string s)
        {
            if (double.TryParse(s, out var value))
            {
                return value;
            }

            Debug.LogWarning($"Argument captured, but not number format. value: {s}");
            return default(double);
        }
    }
}
