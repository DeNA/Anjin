// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

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
    }
}
