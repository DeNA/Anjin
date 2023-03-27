// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Diagnostics.CodeAnalysis;

namespace DeNA.Anjin.Utilities
{
    /// <summary>
    /// Pseudo-random number generator for use with Agents
    /// </summary>
    public interface IRandom
    {
        /// <summary>
        /// Returns a non-negative random integer value.
        /// </summary>
        /// <returns>Random value that 0 to less than Int32.MaxValue</returns>
        int Next();

        /// <summary>
        /// Returns a non-negative random integer value that less than specified value.
        /// </summary>
        /// <param name="maxValue"></param>
        /// <returns>Random value that 0 to less than specific value</returns>
        int Next(int maxValue);

        /// <summary>
        /// Return random integer value that is within a specified range.
        /// 指定範囲内のランダムな整数を返します
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns>Random value that is within a specified range (maxValue is exclusive)</returns>
        int Next(int minValue, int maxValue);
    }

    /// <inheritdoc />
    public class Random : IRandom
    {
        private readonly System.Random _randomImpl;

        /// <summary>
        /// Generate a pseudo-random number generator instance by specifying the seed.
        /// For power equality, create and use an instance for each Agent with <c>RandomFactory</c>.
        /// </summary>
        /// <param name="seed"></param>
        public Random(int seed)
        {
            _randomImpl = new System.Random(seed);
        }

        /// <summary>
        /// Use <c>Random(int)</c>
        /// </summary>
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private Random()
        {
            _randomImpl = new System.Random();
        }

        /// <inheritdoc cref="IRandom.Next()" />
        public int Next()
        {
            return _randomImpl.Next();
        }

        /// <inheritdoc cref="IRandom.Next(int)" />
        public int Next(int maxValue)
        {
            return _randomImpl.Next(maxValue);
        }

        /// <inheritdoc cref="IRandom.Next(int,int)" />
        public int Next(int minValue, int maxValue)
        {
            return _randomImpl.Next(minValue, maxValue);
        }
    }
}
