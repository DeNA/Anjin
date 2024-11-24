// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Settings;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// ReSharper disable MethodHasAsyncOverload

namespace DeNA.Anjin.Loggers
{
    [TestFixture]
    public class FileLoggerAssetTest
    {
        private static string OutputDirectory =>
            Path.Combine(Application.temporaryCachePath, TestContext.CurrentContext.Test.ClassName);

        private static string LogFilePath =>
            Path.Combine(OutputDirectory, $"{TestContext.CurrentContext.Test.Name}.log");

        private const string TimestampRegex = @"\[\d{2}:\d{2}:\d{2}\.\d{3}\] ";

        private static Exception CreateExceptionWithStacktrace(string message)
        {
            try
            {
                throw new ApplicationException(message);
            }
            catch (Exception e)
            {
                return e;
            }
        }

        [SetUp]
        public void SetUp()
        {
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            settings.outputRootPath = OutputDirectory;
            AutopilotState.Instance.settings = settings;
        }

        [TearDown]
        public void TearDown()
        {
            AutopilotState.Instance.Reset();
        }

        [Test, Order(0)]
        public async Task GetLogger_DirectoryDoesNotExist_CreateDirectoryAndWriteToFile()
        {
            if (Directory.Exists(OutputDirectory))
            {
                Directory.Delete(OutputDirectory, true);
            }

            var message = TestContext.CurrentContext.Test.Name;
            var path = LogFilePath;
            var sut = ScriptableObject.CreateInstance<FileLoggerAsset>();
            sut.outputPath = path;
            sut.timestamp = false;

            sut.Logger.Log(message);
            sut.Dispose();
            await Task.Yield();

            var actual = File.ReadAllText(path);
            Assert.That(actual, Is.EqualTo(message + Environment.NewLine));
        }

        [Test]
        public async Task GetLogger_FileExists_OverwrittenToFile()
        {
            var message = TestContext.CurrentContext.Test.Name;
            var path = LogFilePath;
            File.WriteAllText(path, "Existing content");

            var sut = ScriptableObject.CreateInstance<FileLoggerAsset>();
            sut.outputPath = path;
            sut.timestamp = false;

            sut.Logger.Log(message);
            sut.Dispose();
            await Task.Yield();

            var actual = File.ReadAllText(path);
            Assert.That(actual, Is.EqualTo(message + Environment.NewLine));
        }

        [Test]
        public void GetLogger_OutputPathIsEmpty_ReturnsNull()
        {
            var path = string.Empty;
            var sut = ScriptableObject.CreateInstance<FileLoggerAsset>();
            sut.outputPath = path;
            sut.timestamp = false;

            Assert.That(sut.Logger, Is.Null);
            LogAssert.Expect(LogType.Warning, "File Logger output path is not set.");
        }

        [Test]
        public async Task GetLogger_OutputPathIsAbsolutePath_WriteToFile()
        {
            var message = TestContext.CurrentContext.Test.Name;
            var path = Path.Combine(Application.temporaryCachePath, $"{TestContext.CurrentContext.Test.Name}.log");
            var sut = ScriptableObject.CreateInstance<FileLoggerAsset>();
            sut.outputPath = path;
            sut.timestamp = false;

            sut.Logger.Log(message);
            sut.Dispose();
            await Task.Yield();

            var actual = File.ReadAllText(path);
            Assert.That(actual, Is.EqualTo(message + Environment.NewLine));

            // teardown
            File.Delete(path);
        }

        [Test]
        public async Task LogFormat_WriteToFile()
        {
            var message = TestContext.CurrentContext.Test.Name;
            var path = LogFilePath;
            var sut = ScriptableObject.CreateInstance<FileLoggerAsset>();
            sut.outputPath = path;
            sut.timestamp = false;

            sut.Logger.Log(message);
            sut.Logger.Log(message);
            sut.Dispose();
            await Task.Yield();

            var actual = File.ReadAllText(path);
            Assert.That(actual, Is.EqualTo(message + Environment.NewLine + message + Environment.NewLine));
        }

        [Test]
        public async Task LogFormat_Disposed_NotWriteToFile()
        {
            var message = TestContext.CurrentContext.Test.Name;
            var path = LogFilePath;
            var sut = ScriptableObject.CreateInstance<FileLoggerAsset>();
            sut.outputPath = path;
            sut.timestamp = false;

            sut.Logger.Log(message);
            sut.Logger.Log(message);
            sut.Dispose();
            await Task.Yield();

            sut.Logger.Log(message); // write after disposed
            await Task.Yield();

            var actual = File.ReadAllText(path);
            Assert.That(actual, Is.EqualTo(message + Environment.NewLine + message + Environment.NewLine));
        }

        [Test]
        public async Task LogFormat_WithTimestamp_WriteTimestamp()
        {
            var message = TestContext.CurrentContext.Test.Name;
            var path = LogFilePath;
            var sut = ScriptableObject.CreateInstance<FileLoggerAsset>();
            sut.outputPath = path;
            sut.timestamp = true;

            sut.Logger.Log(message);
            await UniTask.NextFrame();
            sut.Logger.Log(message);
            sut.Logger.Log(message); // using cache
            sut.Dispose();
            await Task.Yield();

            var actual = File.ReadAllText(path);
            Assert.That(actual, Does.Match(
                "^" +
                TimestampRegex + message + Environment.NewLine +
                TimestampRegex + message + Environment.NewLine +
                TimestampRegex + message + Environment.NewLine +
                "$"));
        }

        [Test]
        public async Task LogException_WriteToFile()
        {
            var message = TestContext.CurrentContext.Test.Name;
            var path = LogFilePath;
            var sut = ScriptableObject.CreateInstance<FileLoggerAsset>();
            sut.outputPath = path;
            sut.timestamp = false;

            var exception = CreateExceptionWithStacktrace(message);
            sut.Logger.LogException(exception);
            sut.Dispose();
            await Task.Yield();

            var actual = File.ReadAllText(path);
            Assert.That(actual, Is.EqualTo(exception + Environment.NewLine));
        }

        [Test]
        public async Task LogException_Disposed_NotWriteToFile()
        {
            var message = TestContext.CurrentContext.Test.Name;
            var path = LogFilePath;
            var sut = ScriptableObject.CreateInstance<FileLoggerAsset>();
            sut.outputPath = path;
            sut.timestamp = false;

            var exception = CreateExceptionWithStacktrace(message);
            sut.Logger.LogException(exception);
            sut.Dispose();
            await Task.Yield();

            sut.Logger.LogException(exception); // write after disposed
            await Task.Yield();

            var actual = File.ReadAllText(path);
            Assert.That(actual, Is.EqualTo(exception + Environment.NewLine));
        }

        [Test]
        public async Task LogException_WithTimestamp_WriteTimestamp()
        {
            var message = TestContext.CurrentContext.Test.Name;
            var path = LogFilePath;
            var sut = ScriptableObject.CreateInstance<FileLoggerAsset>();
            sut.outputPath = path;
            sut.timestamp = true;

            var exception = CreateExceptionWithStacktrace(message);
            sut.Logger.LogException(exception);
            sut.Dispose();
            await Task.Yield();

            var actual = File.ReadAllText(path);
            Assert.That(actual, Does.Match("^" + TimestampRegex + ".*"));
        }

        [Test]
        public async Task ResetLoggers_ResetLoggerAsset()
        {
            var path = LogFilePath;
            var sut = ScriptableObject.CreateInstance<FileLoggerAsset>();
            sut.outputPath = path;
            sut.Logger.Log("Before reset");
            sut.Dispose();
            await Task.Yield();
            Assume.That(path, Does.Exist);

            File.Delete(path);
            FileLoggerAsset.ResetLoggers(); // Called when on launch autopilot

            sut.Logger.Log("After reset");
            sut.Dispose();
            await Task.Yield();
            Assert.That(path, Does.Exist);
        }
    }
}
