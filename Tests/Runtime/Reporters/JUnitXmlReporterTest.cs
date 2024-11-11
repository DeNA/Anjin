﻿// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using DeNA.Anjin.Settings;
using DeNA.Anjin.TestDoubles;
using NUnit.Framework;
using TestHelper.Comparers;
using UnityEngine;

namespace DeNA.Anjin.Reporters
{
    [TestFixture]
    public class JUnitXmlReporterTest
    {
        [Test]
        [Category("IgnoreCI")]
        public void GetOutputPath_WithoutArg_ReturnsFieldValue()
        {
            var arguments = new StubArguments
            {
                _jUnitReportPath = new StubArgument<string>() // Not captured
            };

            var actual = JUnitXmlReporter.GetOutputPath("Path/By/Field", arguments);
            var expected = Path.GetFullPath("Path/By/Field");
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetOutputPath_WithArg_ReturnsArgValue()
        {
            var arguments = new StubArguments
            {
                _jUnitReportPath = new StubArgument<string>(true, "/Path/By/Arg") // Captured
            };

            var actual = JUnitXmlReporter.GetOutputPath("Path/By/Field", arguments);
            Assert.That(actual, Is.EqualTo("/Path/By/Arg")); // absolute path
        }

        [Test]
        public void CreateTestCase_Passed()
        {
            var name = TestContext.CurrentContext.Test.Name;
            var time = 5.5f;
            var exitCode = ExitCode.Normally;
            var actual = JUnitXmlReporter.CreateTestCase(name, time, null, null, exitCode);

            var expected = new XElement("testcase",
                new XAttribute("name", name),
                new XAttribute("classname", "DeNA.Anjin.Autopilot"), // Note: fixed value
                new XAttribute("time", time),
                new XAttribute("status", "Passed")
            );

            Assert.That(new XDocument(actual), Is.EqualTo(new XDocument(expected)).Using(new XDocumentComparer()));
        }

        [Test]
        public void CreateTestCase_Error()
        {
            var name = TestContext.CurrentContext.Test.Name;
            var time = 5.5f;
            var message = "message";
            var stackTrace = "stack trace";
            var exitCode = ExitCode.UnCatchExceptions;
            var actual = JUnitXmlReporter.CreateTestCase(name, time, message, stackTrace, exitCode);

            var expected = new XElement("testcase",
                new XAttribute("name", name),
                new XAttribute("classname", "DeNA.Anjin.Autopilot"), // Note: fixed value
                new XAttribute("time", time),
                new XAttribute("status", "Failed"),
                new XElement("error",
                    new XAttribute("message", message),
                    new XAttribute("type", "UnCatchExceptions"),
                    new XCData(stackTrace)
                )
            );

            Assert.That(new XDocument(actual), Is.EqualTo(new XDocument(expected)).Using(new XDocumentComparer()));
        }

        [Test]
        public void CreateTestCase_Failure()
        {
            var name = TestContext.CurrentContext.Test.Name;
            var time = 5.5f;
            var message = "message";
            var stackTrace = "stack trace";
            var exitCode = ExitCode.AutopilotFailed;
            var actual = JUnitXmlReporter.CreateTestCase(name, time, message, stackTrace, exitCode);

            var expected = new XElement("testcase",
                new XAttribute("name", name),
                new XAttribute("classname", "DeNA.Anjin.Autopilot"), // Note: fixed value
                new XAttribute("time", time),
                new XAttribute("status", "Failed"),
                new XElement("failure",
                    new XAttribute("message", message),
                    new XAttribute("type", "AutopilotFailed"),
                    new XCData(stackTrace)
                )
            );

            Assert.That(new XDocument(actual), Is.EqualTo(new XDocument(expected)).Using(new XDocumentComparer()));
        }

        [Test]
        public void CreateTestSuite_Passed()
        {
            var name = TestContext.CurrentContext.Test.Name;
            var timestamp = new DateTime(2023, 1, 2, 3, 4, 5);
            var testcase = new XElement("testcase",
                new XAttribute("time", 5.5)
            );
            var actual = JUnitXmlReporter.CreateTestSuite(name, timestamp, testcase);

            var expected = new XElement("testsuite",
                new XAttribute("name", name),
                new XAttribute("tests", 1),
                new XAttribute("id", 0), // Note: fixed value
                new XAttribute("disabled", 0), // Note: fixed value
                new XAttribute("errors", 0),
                new XAttribute("failures", 0),
                new XAttribute("skipped", 0), // Note: fixed value
                new XAttribute("time", 5.5),
                new XAttribute("timestamp", timestamp.ToString("2023-01-02T03:04:05")),
                testcase
            );

            Assert.That(new XDocument(actual), Is.EqualTo(new XDocument(expected)).Using(new XDocumentComparer()));
        }

        [Test]
        public void CreateTestSuite_Error()
        {
            var name = TestContext.CurrentContext.Test.Name;
            var timestamp = new DateTime(2023, 1, 2, 3, 4, 5);
            var testcase = new XElement("testcase",
                new XAttribute("time", 5.5),
                new XElement("error")
            );
            var actual = JUnitXmlReporter.CreateTestSuite(name, timestamp, testcase);

            var expected = new XElement("testsuite",
                new XAttribute("name", name),
                new XAttribute("tests", 1),
                new XAttribute("id", 0), // Note: fixed value
                new XAttribute("disabled", 0), // Note: fixed value
                new XAttribute("errors", 1),
                new XAttribute("failures", 0),
                new XAttribute("skipped", 0), // Note: fixed value
                new XAttribute("time", 5.5),
                new XAttribute("timestamp", timestamp.ToString("2023-01-02T03:04:05")),
                testcase
            );

            Assert.That(new XDocument(actual), Is.EqualTo(new XDocument(expected)).Using(new XDocumentComparer()));
        }

        [Test]
        public void CreateTestSuite_Failure()
        {
            var name = TestContext.CurrentContext.Test.Name;
            var timestamp = new DateTime(2023, 1, 2, 3, 4, 5);
            var testcase = new XElement("testcase",
                new XAttribute("time", 5.5),
                new XElement("failure")
            );
            var actual = JUnitXmlReporter.CreateTestSuite(name, timestamp, testcase);

            var expected = new XElement("testsuite",
                new XAttribute("name", name),
                new XAttribute("tests", 1),
                new XAttribute("id", 0), // Note: fixed value
                new XAttribute("disabled", 0), // Note: fixed value
                new XAttribute("errors", 0),
                new XAttribute("failures", 1),
                new XAttribute("skipped", 0), // Note: fixed value
                new XAttribute("time", 5.5),
                new XAttribute("timestamp", timestamp.ToString("2023-01-02T03:04:05")),
                testcase
            );

            Assert.That(new XDocument(actual), Is.EqualTo(new XDocument(expected)).Using(new XDocumentComparer()));
        }

        [Test]
        public void CreateTestSuites_Passed()
        {
            var name = TestContext.CurrentContext.Test.Name;
            var timestamp = new DateTime(2023, 1, 2, 3, 4, 5);
            var testcase = new XElement("testcase",
                new XAttribute("time", 5.5)
            );
            var testsuite = JUnitXmlReporter.CreateTestSuite(string.Empty, timestamp, testcase);
            var actual = JUnitXmlReporter.CreateTestSuites(name, testsuite);

            var expected = new XElement("testsuites",
                new XAttribute("name", name),
                new XAttribute("disabled", 0), // Note: fixed value
                new XAttribute("errors", 0),
                new XAttribute("failures", 0),
                new XAttribute("tests", 1),
                new XAttribute("time", 5.5),
                testsuite
            );

            Assert.That(new XDocument(actual), Is.EqualTo(new XDocument(expected)).Using(new XDocumentComparer()));
        }

        [Test]
        public void CreateTestSuites_Error()
        {
            var name = TestContext.CurrentContext.Test.Name;
            var timestamp = new DateTime(2023, 1, 2, 3, 4, 5);
            var testcase = new XElement("testcase",
                new XAttribute("time", 5.5),
                new XElement("error")
            );
            var testsuite = JUnitXmlReporter.CreateTestSuite(string.Empty, timestamp, testcase);
            var actual = JUnitXmlReporter.CreateTestSuites(name, testsuite);

            var expected = new XElement("testsuites",
                new XAttribute("name", name),
                new XAttribute("disabled", 0), // Note: fixed value
                new XAttribute("errors", 1),
                new XAttribute("failures", 0),
                new XAttribute("tests", 1),
                new XAttribute("time", 5.5),
                testsuite
            );

            Assert.That(new XDocument(actual), Is.EqualTo(new XDocument(expected)).Using(new XDocumentComparer()));
        }

        [Test]
        public void CreateTestSuites_Failure()
        {
            var name = TestContext.CurrentContext.Test.Name;
            var timestamp = new DateTime(2023, 1, 2, 3, 4, 5);
            var testcase = new XElement("testcase",
                new XAttribute("time", 5.5),
                new XElement("failure")
            );
            var testsuite = JUnitXmlReporter.CreateTestSuite(string.Empty, timestamp, testcase);
            var actual = JUnitXmlReporter.CreateTestSuites(name, testsuite);

            var expected = new XElement("testsuites",
                new XAttribute("name", name),
                new XAttribute("disabled", 0), // Note: fixed value
                new XAttribute("errors", 0),
                new XAttribute("failures", 1),
                new XAttribute("tests", 1),
                new XAttribute("time", 5.5),
                testsuite
            );

            Assert.That(new XDocument(actual), Is.EqualTo(new XDocument(expected)).Using(new XDocumentComparer()));
        }

        [Test]
        public async Task PostReportAsync_Error()
        {
            AutopilotState.Instance.settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            AutopilotState.Instance.settings.name = TestContext.CurrentContext.Test.Name;

            JUnitXmlReporter.Initialize();
            var startDatetime = DateTime.Now; // Note: same as the value that JUnitXmlReporter has, maybe.

            var outputPath = CreateTestOutputPath();
            var sut = ScriptableObject.CreateInstance<JUnitXmlReporter>();
            sut.outputPath = outputPath;

            await sut.PostReportAsync("MESSAGE", "STACK TRACE", ExitCode.UnCatchExceptions);
            Assert.That(outputPath, Does.Exist);

            var actual = File.ReadAllText(outputPath);
            var expected = @"<?xml version=""1.0"" encoding=""utf-8""?>
<testsuites name=""DeNA.Anjin""
            disabled=""0""
            errors=""1""
            failures=""0""
            tests=""1""
            time=""REPLACE_TIME""
            >
            <testsuite  name=""DeNA.Anjin""
                        tests=""1""
                        id=""0""
                        disabled=""0""
                        errors=""1""
                        failures=""0""
                        skipped=""0""
                        time=""REPLACE_TIME""
                        timestamp=""REPLACE_TIMESTAMP""
                        >
                        <testcase   name=""REPLACE_TESTCASE_NAME""
                                    classname=""DeNA.Anjin.Autopilot""
                                    time=""REPLACE_TIME""
                                    status=""Failed""
                                    >
                                    <error message=""MESSAGE""
                                           type=""UnCatchExceptions""
                                            ><![CDATA[STACK TRACE]]></error>
                        </testcase>
            </testsuite>
</testsuites>"
                .Replace("REPLACE_TESTCASE_NAME", TestContext.CurrentContext.Test.Name)
                .Replace("REPLACE_TIMESTAMP", startDatetime.ToString("yyyy-MM-ddTHH:mm:ss"))
                .Replace("REPLACE_TIME", "0");

            Debug.Log($"expected: {expected}");

            Assert.That(actual, Is.EqualTo(expected).Using(new XmlComparer()));

            // teardown
            AutopilotState.Instance.Reset();
        }

        private static string CreateTestOutputPath()
        {
            var dir = Path.Combine(Application.temporaryCachePath, TestContext.CurrentContext.Test.ClassName);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var path = Path.Combine(dir, $"{TestContext.CurrentContext.Test.Name}.xml");
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            return path;
        }
    }
}
