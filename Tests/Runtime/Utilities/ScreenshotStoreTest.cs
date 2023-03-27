// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace DeNA.Anjin.Utilities
{
    public class ScreenshotStoreTest
    {
        private static string rootPath => Path.Combine(Application.persistentDataPath, "Anjin");

        [Test]
        public void CleanDirectories_CleanDirectories()
        {
            var subPath = Path.Combine(rootPath, "Sub");
            Directory.CreateDirectory(subPath);

            ScreenshotStore.CleanDirectories();

            Assert.That(rootPath, Does.Exist.IgnoreFiles);
            Assert.That(subPath, Does.Not.Exist);
        }

        [Test]
        public void CleanDirectories_CreateDirectories()
        {
            Directory.Delete(rootPath, true);

            ScreenshotStore.CleanDirectories();

            Assert.That(rootPath, Does.Exist.IgnoreFiles);
        }

        [Test]
        public void CreateDirectory_WithAgentName_CreateDirectory()
        {
            ScreenshotStore.CleanDirectories();

            var dateTime = new DateTime(2022, 3, 4, 5, 6, 7);
            ScreenshotStore.CreateDirectory(dateTime, "FooAgent");

            var expectedPath = Path.Combine(rootPath, "20220304050607_FooAgent");
            Assert.That(expectedPath, Does.Exist.IgnoreFiles);
        }

        [Test]
        public void CreateDirectory_WithoutAgentName_CreateDirectory()
        {
            ScreenshotStore.CleanDirectories();

            var dateTime = new DateTime(2022, 3, 4, 5, 6, 7);
            ScreenshotStore.CreateDirectory(dateTime);

            var expectedPath = Path.Combine(rootPath, "20220304050607");
            Assert.That(expectedPath, Does.Exist.IgnoreFiles);
        }
    }
}
