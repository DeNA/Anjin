// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace DeNA.Anjin.Utilities
{
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public class JUnitReporterTest
    {
        private string _path;

        [SetUp]
        public void SetUp()
        {
            _path = Path.Combine(Application.persistentDataPath, "result.xml");
            if (File.Exists(_path))
            {
                File.Delete(_path);
            }
        }

        [Test]
        public void Output_success_wroteJUnitXmlReportingFile()
        {
            JUnitReporter.Output(_path, 0);

            Assert.That(File.ReadAllText(_path), Is.EqualTo(@"<?xml version=""1.0"" encoding=""UTF-8""?>
<testsuites name=""Anjin""
            disabled=""0""
            errors=""0""
            failures=""0""
            tests=""1""
            time=""0.000""
            >
            <testsuite  name=""DeNA.Anjin""
                        tests=""1""
                        id=""0""
                        >
                        <testcase   name=""Autopilot""
                                    classname=""DeNA.Anjin""
                                    >
                        </testcase>
            </testsuite>
</testsuites>"));
        }

        [Test]
        public void Output_successWithTime_wroteJUnitXmlReportingFileIncludeTime()
        {
            JUnitReporter.Output(_path, 0, null, null, 300.3333f);

            Assert.That(File.ReadAllText(_path), Is.EqualTo(@"<?xml version=""1.0"" encoding=""UTF-8""?>
<testsuites name=""Anjin""
            disabled=""0""
            errors=""0""
            failures=""0""
            tests=""1""
            time=""300.333""
            >
            <testsuite  name=""DeNA.Anjin""
                        tests=""1""
                        id=""0""
                        >
                        <testcase   name=""Autopilot""
                                    classname=""DeNA.Anjin""
                                    >
                        </testcase>
            </testsuite>
</testsuites>"));
        }

        [Test]
        public void Output_error_wroteJUnitXmlReportingFile()
        {
            JUnitReporter.Output(_path, 1, "Error message!", "Stack trace!");

            Assert.That(File.ReadAllText(_path), Is.EqualTo(@"<?xml version=""1.0"" encoding=""UTF-8""?>
<testsuites name=""Anjin""
            disabled=""0""
            errors=""1""
            failures=""0""
            tests=""1""
            time=""0.000""
            >
            <testsuite  name=""DeNA.Anjin""
                        tests=""1""
                        id=""0""
                        >
                        <testcase   name=""Autopilot""
                                    classname=""DeNA.Anjin""
                                    >
                                    <error message=""Error message!""
                                           type=""""
                                            >Stack trace!</error>
                        </testcase>
            </testsuite>
</testsuites>"));
        }

        [Test]
        public void Output_failure_wroteJUnitXmlReportingFile()
        {
            JUnitReporter.Output(_path, 2, "Failure message!", "Stack trace!!");

            Assert.That(File.ReadAllText(_path), Is.EqualTo(@"<?xml version=""1.0"" encoding=""UTF-8""?>
<testsuites name=""Anjin""
            disabled=""0""
            errors=""0""
            failures=""1""
            tests=""1""
            time=""0.000""
            >
            <testsuite  name=""DeNA.Anjin""
                        tests=""1""
                        id=""0""
                        >
                        <testcase   name=""Autopilot""
                                    classname=""DeNA.Anjin""
                                    >
                                    <failure message=""Failure message!""
                                             type=""""
                                            >Stack trace!!</failure>
                        </testcase>
            </testsuite>
</testsuites>"));
        }
    }
}
