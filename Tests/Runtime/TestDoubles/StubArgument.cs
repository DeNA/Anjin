// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using DeNA.Anjin.ArgumentCapture;

namespace DeNA.Anjin.TestDoubles
{
    public class StubArgument<T> : IArgument<T>
    {
        private readonly bool _captured;
        private readonly T _value;

        public StubArgument(bool captured = false, T value = default(T))
        {
            _captured = captured;
            _value = value;
        }

        public bool IsCaptured()
        {
            return _captured;
        }

        public T Value()
        {
            return _value;
        }
    }
}
