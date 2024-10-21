// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using NUnit.Framework;

namespace DeNA.Anjin.Strategies
{
    public class TwoTieredCounterStrategyTest
    {
        [SetUp]
        public void SetUp()
        {
            TwoTieredCounterStrategy.ResetPrefixCounters();
        }

        [Test]
        public void GetFilename_WithPrefix_ReturnsUniqueFilename()
        {
            var sut = new TwoTieredCounterStrategy("MyPrefix");

            Assert.That(sut.GetFilename(), Is.EqualTo("MyPrefix01_0001.png"));
            Assert.That(sut.GetFilename(), Is.EqualTo("MyPrefix01_0002.png"));
        }

        [Test]
        public void GetFilename_WithoutPrefix_ReturnsFilenameUsingTestName()
        {
            var sut = new TwoTieredCounterStrategy();

            Assert.That(sut.GetFilename(), Is.EqualTo($"{TestContext.CurrentContext.Test.Name}01_0001.png"));
            Assert.That(sut.GetFilename(), Is.EqualTo($"{TestContext.CurrentContext.Test.Name}01_0002.png"));
        }

        [Test]
        public void GetFilename_WithSamePrefix_ReturnsFilenameUsingUniqueNumberByPrefix()
        {
            var sut = new TwoTieredCounterStrategy("MyPrefix");
            var sut2 = new TwoTieredCounterStrategy("MyPrefix");

            Assert.That(sut.GetFilename(), Is.EqualTo("MyPrefix01_0001.png"));
            Assert.That(sut.GetFilename(), Is.EqualTo("MyPrefix01_0002.png"));
            Assert.That(sut2.GetFilename(), Is.EqualTo("MyPrefix02_0001.png"));
            Assert.That(sut2.GetFilename(), Is.EqualTo("MyPrefix02_0002.png"));
        }

        [Test]
        public void GetFilename_WithAnotherPrefix_ReturnsUniqueFilenameByPrefix()
        {
            var sut = new TwoTieredCounterStrategy("MyPrefix");
            var sut2 = new TwoTieredCounterStrategy("AnotherPrefix");

            Assert.That(sut.GetFilename(), Is.EqualTo("MyPrefix01_0001.png"));
            Assert.That(sut.GetFilename(), Is.EqualTo("MyPrefix01_0002.png"));
            Assert.That(sut2.GetFilename(), Is.EqualTo("AnotherPrefix01_0001.png"));
            Assert.That(sut2.GetFilename(), Is.EqualTo("AnotherPrefix01_0002.png"));
        }
    }
}
