// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using NUnit.Framework;
using TestHelper.Attributes;
using UnityEngine.TestTools.Utils;

namespace DeNA.Anjin.ArgumentCapture
{
    /// <summary>
    /// Test arguments when launch from the command line.
    /// This is actually a test of <c>ArgumentCapture</c>.
    /// </summary>
    [TestFixture]
    public class ArgumentTest
    {
        [Test]
        public void String_inArgument_gotValue()
        {
            var arg = new Argument<string>("STR_ARG", args: new[] { "-STR_ARG", "STRING_BY_ARGUMENT" });
            Assert.That(arg.IsCaptured, Is.True);
            Assert.That(arg.Value(), Is.EqualTo("STRING_BY_ARGUMENT"));
        }

        [Test]
        [IgnoreWindowMode("Need command line arguments. see Makefile")]
        public void String_inEnvironmentVariable_gotValue()
        {
            var arg = new Argument<string>("STR_ENV");
            Assert.That(arg.IsCaptured, Is.True);
            Assert.That(arg.Value(), Is.EqualTo("STRING_BY_ENVIRONMENT_VARIABLE"));
        }

        [Test]
        public void String_notExist_gotDefault()
        {
            const string Expected = "DEFAULT";
            var arg = new Argument<string>("STR_NOT_SPECIFIED", Expected);
            Assert.That(arg.IsCaptured, Is.False);
            Assert.That(arg.Value(), Is.EqualTo(Expected));
        }

        [Test]
        public void Bool_inArgumentTrue_gotTrue()
        {
            var arg = new Argument<bool>("BOOL_ARG_TRUE", args: new[] { "-BOOL_ARG_TRUE", "TRUE" });
            Assert.That(arg.IsCaptured, Is.True);
            Assert.That(arg.Value(), Is.True);
        }

        [Test]
        public void Bool_inArgumentFalse_gotFalse()
        {
            var arg = new Argument<bool>("BOOL_ARG_FALSE", args: new[] { "-BOOL_ARG_FALSE", "FALSE" });
            Assert.That(arg.IsCaptured, Is.True);
            Assert.That(arg.Value(), Is.False);
        }

        [Test]
        public void Bool_inArgumentNoValue_HasNextArgumentKey_gotTrue()
        {
            var arg = new Argument<bool>("BOOL_ARG_NO_VALUE", args: new[] { "-BOOL_ARG_NO_VALUE", "-NEXT_ARG" });
            Assert.That(arg.IsCaptured, Is.True);
            Assert.That(arg.Value(), Is.True);
        }

        [Test]
        public void Bool_inArgumentNoValue_NoNextArgumentKey_gotTrue()
        {
            var arg = new Argument<bool>("BOOL_ARG_NO_VALUE", args: new[] { "-BOOL_ARG_NO_VALUE" });
            Assert.That(arg.IsCaptured, Is.True);
            Assert.That(arg.Value(), Is.True);
        }

        [Test]
        public void Bool_invalidValue_gotDefault()
        {
            var arg = new Argument<bool>("STR_ARG", args: new[] { "-STR_ARG", "STRING_BY_ARGUMENT" });
            Assert.That(arg.IsCaptured, Is.True);
            Assert.That(arg.Value(), Is.EqualTo(default(bool)));
        }

        [Test]
        public void Int_inArgument_gotValue()
        {
            var arg = new Argument<int>("INT_ARG_1", args: new[] { "-INT_ARG_1", "1" });
            Assert.That(arg.IsCaptured, Is.True);
            Assert.That(arg.Value(), Is.EqualTo(1));
        }

        [Test]
        public void Int_invalidValue_gotDefault()
        {
            var arg = new Argument<int>("STR_ARG", args: new[] { "-STR_ARG", "STRING_BY_ARGUMENT" });
            Assert.That(arg.IsCaptured, Is.True);
            Assert.That(arg.Value(), Is.EqualTo(default(int)));
        }

        [Test]
        public void Long_inArgument_gotValue()
        {
            var arg = new Argument<long>("LONG_ARG_MAX", args: new[] { "-LONG_ARG_MAX", "9223372036854775807" });
            Assert.That(arg.IsCaptured, Is.True);
            Assert.That(arg.Value(), Is.EqualTo(long.MaxValue));
        }

        [Test]
        public void Long_invalidValue_gotDefault()
        {
            var arg = new Argument<long>("STR_ARG", args: new[] { "-STR_ARG", "STRING_BY_ARGUMENT" });
            Assert.That(arg.IsCaptured, Is.True);
            Assert.That(arg.Value(), Is.EqualTo(default(long)));
        }

        [Test]
        public void Float_inArgument_gotValue()
        {
            var arg = new Argument<float>("FLOAT_ARG_2_3", args: new[] { "-FLOAT_ARG_2_3", "2.3" });
            Assert.That(arg.IsCaptured, Is.True);
            Assert.That(arg.Value(), Is.EqualTo(2.3f).Using(FloatEqualityComparer.Instance));
        }

        [Test]
        public void Float_invalidValue_gotDefault()
        {
            var arg = new Argument<float>("STR_ARG", args: new[] { "-STR_ARG", "STRING_BY_ARGUMENT" });
            Assert.That(arg.IsCaptured, Is.True);
            Assert.That(arg.Value(), Is.EqualTo(default(float)));
        }

        [Test]
        public void Double_inArgument_gotValue()
        {
            var arg = new Argument<double>("DOUBLE_ARG_4_5", args: new[] { "-DOUBLE_ARG_4_5", "4.5" });
            Assert.That(arg.IsCaptured, Is.True);
            Assert.That(arg.Value(), Is.EqualTo(4.5d).Using(FloatEqualityComparer.Instance));
        }

        [Test]
        public void Double_invalidValue_gotDefault()
        {
            var arg = new Argument<double>("STR_ARG", args: new[] { "-STR_ARG", "STRING_BY_ARGUMENT" });
            Assert.That(arg.IsCaptured, Is.True);
            Assert.That(arg.Value(), Is.EqualTo(default(double)));
        }
    }
}
