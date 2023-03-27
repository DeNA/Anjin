// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using DeNA.Anjin.Utilities;

namespace DeNA.Anjin.TestDoubles
{
    public class StubRandom : IRandom
    {
        private readonly Func<int, int, int> _nextDelegate;

        public StubRandom(Func<int, int, int> rangeDelegate)
        {
            _nextDelegate = rangeDelegate;
        }

        public int Next()
        {
            throw new System.NotImplementedException();
        }

        public int Next(int maxValue)
        {
            throw new System.NotImplementedException();
        }

        public int Next(int minValue, int maxValue)
        {
            return _nextDelegate(minValue, maxValue);
        }
    }
}
