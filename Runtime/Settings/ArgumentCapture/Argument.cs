// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

namespace DeNA.Anjin.Settings.ArgumentCapture
{
    /// <summary>
    /// Capture commandline arguments or environment variables
    /// </summary>
    /// <remarks>
    /// Note:
    ///  - IsCaptured() returns false if the specified KEY argument is unspecified
    ///  - Type cast failures, etc. are logged and IsCaptured() returns false
    ///  - IsCaptured() returns false if type cast failures
    /// </remarks>
    /// <typeparam name="T">Argument type</typeparam>
    internal class Argument<T> : IArgument<T>
    {
        private readonly T _defaultValue;
        private readonly (bool, T) _captured;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="key">Key of arguments</param>
        /// <param name="defaultValue">Use this value if not specified key in arguments</param>
        /// <param name="args">Command line arguments for testing</param>
        public Argument(string key, T defaultValue = default, string[] args = null)
        {
            _defaultValue = defaultValue;
            _captured = ArgumentCapture.Capture<T>(key, args);
        }

        /// <inheritdoc/>
        public bool IsCaptured()
        {
            return _captured.Item1;
        }

        /// <inheritdoc/>
        public T Value()
        {
            return _captured.Item1 ? _captured.Item2 : _defaultValue;
        }
    }
}
