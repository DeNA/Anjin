// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

namespace DeNA.Anjin.ArgumentCapture
{
    /// <summary>
    /// Commandline argument
    /// </summary>
    /// <typeparam name="T">Argument type</typeparam>
    public interface IArgument<T>
    {
        /// <summary>
        /// When argument was specified or not
        /// </summary>
        /// <returns>true: argument was specified</returns>
        bool IsCaptured();

        /// <summary>
        /// Argument value
        /// </summary>
        /// <returns>specified value or default value</returns>
        T Value();
    }
}
