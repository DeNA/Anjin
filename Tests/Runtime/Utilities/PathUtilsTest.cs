// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.IO;
using DeNA.Anjin.Settings;
using DeNA.Anjin.TestDoubles;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DeNA.Anjin.Utilities
{
    [TestFixture]
    public class PathUtilsTest
    {
        [Test]
        [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.OSXPlayer, RuntimePlatform.LinuxEditor,
            RuntimePlatform.LinuxPlayer)]
        public void GetAbsolutePath_AbsolutePath_ReturnsPath()
        {
            const string Path = "/Path/To/File.txt";

            var actual = PathUtils.GetAbsolutePath(Path, null);
            Assert.That(actual, Is.EqualTo(Path));
        }

        [Test]
        [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.OSXPlayer, RuntimePlatform.LinuxEditor,
            RuntimePlatform.LinuxPlayer)]
        public void GetAbsolutePath_RelativePath_ReturnsCombinedPath()
        {
            const string Path = "Relative/Path/To/File.txt";
            const string BasePath = "/Base/Path";

            var actual = PathUtils.GetAbsolutePath(Path, BasePath);
            Assert.That(actual, Is.EqualTo("/Base/Path/Relative/Path/To/File.txt"));
        }

        [Test]
        [UnityPlatform(RuntimePlatform.OSXEditor, RuntimePlatform.OSXPlayer, RuntimePlatform.LinuxEditor,
            RuntimePlatform.LinuxPlayer)]
        public void GetAbsolutePath_Empty_ReturnsBasePath()
        {
            const string Path = "";
            const string BasePath = "/Base/Path";

            var actual = PathUtils.GetAbsolutePath(Path, BasePath);
            Assert.That(actual, Is.EqualTo(BasePath));
        }

        [Test]
        [UnityPlatform(RuntimePlatform.WindowsEditor, RuntimePlatform.WindowsPlayer)]
        public void GetAbsolutePath_WindowsAbsolutePath_ReturnsPath()
        {
            const string Path = @"C:\Path\To\File.txt";

            var actual = PathUtils.GetAbsolutePath(Path, null);
            Assert.That(actual, Is.EqualTo(Path));
        }

        [Test]
        public void GetOutputPath_WithoutArg_ReturnsFieldValue()
        {
            var outputDir = Path.Combine(Application.temporaryCachePath, TestContext.CurrentContext.Test.ClassName);
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            settings.outputRootPath = outputDir;
            AutopilotState.Instance.settings = settings;

            var arguments = new StubArguments
            {
                _jUnitReportPath = new StubArgument<string>() // Not captured
            };

            var actual = PathUtils.GetOutputPath("Path/By/Field", arguments.JUnitReportPath);
            var expected = Path.Combine(outputDir, "Path/By/Field");
            Assert.That(actual, Is.EqualTo(expected));

            // teardown
            AutopilotState.Instance.Reset();
        }

        [Test]
        public void GetOutputPath_WithArg_ReturnsArgValue()
        {
            var outputDir = Path.Combine(Application.temporaryCachePath, TestContext.CurrentContext.Test.ClassName);
            var expected = Path.Combine(outputDir, "Path/By/Arg"); // absolute path

            var arguments = new StubArguments
            {
                _jUnitReportPath = new StubArgument<string>(true, expected) // Captured
            };

            var actual = PathUtils.GetOutputPath("Path/By/Field", arguments.JUnitReportPath);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetOutputPath_WithoutArgAndField_ReturnsNull()
        {
            var arguments = new StubArguments
            {
                _jUnitReportPath = new StubArgument<string>() // Not captured
            };

            var actual = PathUtils.GetOutputPath(null, arguments.JUnitReportPath);
            Assert.That(actual, Is.Null);
        }
    }
}
