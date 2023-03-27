// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using NUnit.Framework;

namespace DeNA.Anjin.Utilities
{
    public class RandomFactoryTest
    {
        private readonly int _seed = (int)UnityEngine.Random.value;

        [Test]
        public void CreateRandom_SameSeed_CreatedRandomCanGenerateSameValue()
        {
            var seed = 1000;
            var random1 = new RandomFactory(seed).CreateRandom();
            var random2 = new RandomFactory(seed).CreateRandom();
            Assert.That(random1.Next(), Is.EqualTo(random2.Next()));
        }

        [Test]
        public void CreateRandom_CreatedRandomCanGenerateDifferentValue()
        {
            var seed = 1000;
            var factory = new RandomFactory(seed);
            var random1 = factory.CreateRandom();
            var random2 = factory.CreateRandom();
            Assert.That(random1.Next(), Is.Not.EqualTo(random2.Next())); // They may fail in very rare instances.
        }

        [Test]
        public void ConstructorWithoutArguments_CreateRandomFactoryInstance()
        {
            var random = new RandomFactory().CreateRandom();
            Assert.That(random.Next(), Is.GreaterThanOrEqualTo(0).And.LessThan(int.MaxValue));
        }
    }
}
