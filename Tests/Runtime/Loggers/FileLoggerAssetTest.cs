// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
#if UNITY_EDITOR
            return Path.Combine(LogsDirectoryPath, $"{TestContext.CurrentContext.Test.Name}.log");
#else
            return Path.Combine(Application.persistentDataPath, LogsDirectoryPath,
                $"{TestContext.CurrentContext.Test.Name}.log");
#endif
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
        public async Task GetLogger_DirectoryDoesNotExist_CreateDirectoryAndWriteToFile()
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
        public async Task GetLogger_FileExists_OverwrittenToFile()
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
        public void GetLogger_OutputPathIsEmpty_ThrowsException()
        {
            var message = TestContext.CurrentContext.Test.Name;
            var path = string.Empty;
            var sut = ScriptableObject.CreateInstance<FileLoggerAsset>();
            sut.outputPath = path;
            sut.timestamp = false;

            Assert.That(() => sut.Logger, Throws.TypeOf<InvalidOperationException>()
                .And.Message.EqualTo("outputPath is not set."));
        }

        [Test]
        public async Task GetLogger_OutputPathWithoutDirectory_WriteToFile()
        {
            var message = TestContext.CurrentContext.Test.Name;
            var path = $"{TestContext.CurrentContext.Test.Name}.log"; // without directory
            var sut = ScriptableObject.CreateInstance<FileLoggerAsset>();
            sut.outputPath = path;
            sut.timestamp = false;

            sut.Logger.Log(message);
            sut.Dispose();
            await Task.Yield();

#if UNITY_EDITOR
            var actual = File.ReadAllText(path);
#else
            var actual = File.ReadAllText(Path.Combine(Application.persistentDataPath, path));
#endif
            Assert.That(actual, Is.EqualTo(message + Environment.NewLine));

            // teardown
            File.Delete(path);
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

        [Test]
        public async Task ResetLoggers_ResetLoggerAsset()
        {
            var path = GetOutputPath();
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
