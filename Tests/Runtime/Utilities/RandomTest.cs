// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using NUnit.Framework;

namespace DeNA.Anjin.Utilities
{
    public class RandomTest
    {
        private readonly int _seed = (int)UnityEngine.Random.value;

        [Test]
        public void Next_returnRandomValue()
        {
            var random = new Random(_seed);
            Assert.That(random.Next(), Is.GreaterThanOrEqualTo(0));
            Assert.That(random.Next(), Is.LessThan(int.MaxValue));
        }

        [Test]
        public void NextInt_max1_returnAlwaysZero()
        {
            var random = new Random(_seed);
            Assert.That(random.Next(1), Is.EqualTo(0));
        }

        [Test]
        public void NextIntInt_min1max2_returnAlways1()
        {
            var random = new Random(_seed);
            Assert.That(random.Next(1, 2), Is.EqualTo(1));
        }
    }
}
