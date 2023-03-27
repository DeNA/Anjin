// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;

namespace DeNA.Anjin.Utilities
{
    /// <summary>
    /// Factory class that generate random number generators for use with Agents.
    /// </summary>
    public class RandomFactory
    {
        private readonly System.Random _randomImpl;

        /// <summary>
        /// Constructor without random seed.
        /// </summary>
        [Obsolete("Use the constructor RandomFactory(int) which allows you to specify the seed")]
        public RandomFactory()
        {
            _randomImpl = new System.Random();
        }

        /// <summary>
        /// Constructor with random seed.
        /// </summary>
        /// <param name="seed"></param>
        public RandomFactory(int seed)
        {
            _randomImpl = new System.Random(seed);
        }

        /// <summary>
        /// Create pseudo-random number generator instance.
        /// </summary>
        /// <returns></returns>
        public IRandom CreateRandom()
        {
            return new Random(_randomImpl.Next());
        }
    }
}
