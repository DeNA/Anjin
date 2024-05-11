// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

// ReSharper disable MethodHasAsyncOverload

namespace DeNA.Anjin.Loggers
{
    [TestFixture]
    public class FileLoggerAssetTest
    {
        private const string LogsDirectoryPath = "Logs/FileLoggerTest";
        private const string TimestampRegex = @"\[\d{2}:\d{2}:\d{2}\.\d{3}\] ";

        private static string GetOutputPath()
        {
            return Path.Combine(LogsDirectoryPath, $"{TestContext.CurrentContext.Test.Name}.log");
        }

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

        [Test, Order(0)]
        public async Task GetLoggerImpl_DirectoryDoesNotExist_CreateDirectoryAndWriteToFile()
        {
            if (Directory.Exists(LogsDirectoryPath))
            {
                Directory.Delete(LogsDirectoryPath, true);
            }

            var message = TestContext.CurrentContext.Test.Name;
            var path = GetOutputPath();
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
        public async Task GetLoggerImpl_FileExists_OverwrittenToFile()
        {
            var message = TestContext.CurrentContext.Test.Name;
            var path = GetOutputPath();
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
        public async Task LogFormat_WriteToFile()
        {
            var message = TestContext.CurrentContext.Test.Name;
            var path = GetOutputPath();
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
            var path = GetOutputPath();
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
            var path = GetOutputPath();
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
            var path = GetOutputPath();
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
            var path = GetOutputPath();
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
            var path = GetOutputPath();
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
    }
}
