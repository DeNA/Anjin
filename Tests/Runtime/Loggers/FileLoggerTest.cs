// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace DeNA.Anjin.Loggers
{
    [TestFixture]
    public class FileLoggerTest
    {
        private const string LogsDirectoryPath = "Logs/FileLoggerTest";

        private static string GetOutputPath()
        {
            return Path.Combine(LogsDirectoryPath, $"{TestContext.CurrentContext.Test.Name}.log");
        }

        [Test, Order(0)]
        public async Task GetLoggerImpl_DirectoryDoesNotExist_CreateDirectoryAndWriteToFile()
        {
            Directory.Delete(LogsDirectoryPath, true);

            var message = TestContext.CurrentContext.Test.Name;
            var path = GetOutputPath();
            var sut = ScriptableObject.CreateInstance<FileLogger>();
            sut.outputPath = path;
            sut.timestamp = false;

            sut.LoggerImpl.Log(message);
            sut.Dispose();
            await Task.Yield();

            var actual = await File.ReadAllTextAsync(path);
            Assert.That(actual, Is.EqualTo(message + Environment.NewLine));
        }

        [Test]
        public async Task GetLoggerImpl_FileExists_OverwrittenToFile()
        {
            var message = TestContext.CurrentContext.Test.Name;
            var path = GetOutputPath();
            await File.WriteAllTextAsync(path, "Existing content");

            var sut = ScriptableObject.CreateInstance<FileLogger>();
            sut.outputPath = path;
            sut.timestamp = false;

            sut.LoggerImpl.Log(message);
            sut.Dispose();
            await Task.Yield();

            var actual = await File.ReadAllTextAsync(path);
            Assert.That(actual, Is.EqualTo(message + Environment.NewLine));
        }

        [Test]
        public async Task LogFormat_WriteToFile()
        {
            var message = TestContext.CurrentContext.Test.Name;
            var path = GetOutputPath();
            var sut = ScriptableObject.CreateInstance<FileLogger>();
            sut.outputPath = path;
            sut.timestamp = false;

            sut.LoggerImpl.Log(message);
            sut.LoggerImpl.Log(message);
            sut.Dispose();
            await Task.Yield();

            var actual = await File.ReadAllTextAsync(path);
            Assert.That(actual, Is.EqualTo(message + Environment.NewLine + message + Environment.NewLine));
        }

        [Test]
        public async Task LogException_WriteToFile()
        {
            var message = TestContext.CurrentContext.Test.Name;
            var path = GetOutputPath();
            var sut = ScriptableObject.CreateInstance<FileLogger>();
            sut.outputPath = path;
            sut.timestamp = false;

            Exception exception;
            try
            {
                throw new Exception(message);
            }
            catch (Exception e)
            {
                exception = e;
            }

            sut.LoggerImpl.LogException(exception);
            sut.Dispose();
            await Task.Yield();

            var actual = await File.ReadAllTextAsync(path);
            Assert.That(actual, Does.StartWith(
                "System.Exception: " + message + Environment.NewLine +
                "  at DeNA.Anjin.Loggers.FileLoggerTest.LogException_WriteToFile"));
        }

        [Test]
        public async Task LogFormat_WithTimestamp_WriteTimestamp()
        {
            var message = TestContext.CurrentContext.Test.Name;
            var path = GetOutputPath();
            var sut = ScriptableObject.CreateInstance<FileLogger>();
            sut.outputPath = path;
            sut.timestamp = true;

            sut.LoggerImpl.Log(message);
            await UniTask.NextFrame();
            sut.LoggerImpl.Log(message);
            sut.LoggerImpl.Log(message); // using cache
            sut.Dispose();
            await Task.Yield();

            var actual = await File.ReadAllTextAsync(path);
            var timestampFormat = @"\[\d{2}:\d{2}:\d{2}\.\d{3}\] ";
            var expected = timestampFormat + message + Environment.NewLine +
                           timestampFormat + message + Environment.NewLine +
                           timestampFormat + message + Environment.NewLine;
            Assert.That(actual, Does.Match(expected));
        }
    }
}
