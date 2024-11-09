// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.IO;
using System.Threading;
using System.Xml.Linq;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.Attributes;
using DeNA.Anjin.Settings;
using DeNA.Anjin.Utilities;
using UnityEngine;

namespace DeNA.Anjin.Reporters
{
    /// <summary>
    /// Output JUnit XML format file.
    /// </summary>
    /// <see href="https://github.com/jenkinsci/benchmark-plugin/blob/master/doc/JUnit%20format/JUnit.txt"/>
    [CreateAssetMenu(fileName = "New JUnitXmlReporter", menuName = "Anjin/JUnit XML Reporter", order = 90)]
    public class JUnitXmlReporter : AbstractReporter
    {
        /// <summary>
        /// Output path for JUnit XML format file.
        /// Note: relative path from the project root directory. When run on player, it will be the <c>Application.persistentDataPath</c>.
        /// </summary>
        public string outputPath;

        private static DateTime s_startDatetime;
        private static float s_startTime;

        [InitializeOnLaunchAutopilot]
        internal static void Initialize()
        {
            s_startDatetime = DateTime.Now;
            s_startTime = Time.unscaledTime;
            // Note: Even if there are multiple JUnitXmlReporter instances, these values are common, so they are static.
        }

        /// <inheritdoc/>
        public override async UniTask PostReportAsync(string message, string stackTrace, ExitCode exitCode,
            CancellationToken cancellationToken = default)
        {
            var settings = AutopilotState.Instance.settings;
            if (settings == null)
            {
                throw new InvalidOperationException("Autopilot is not running");
            }

            var path = GetOutputPath(this.outputPath, new Arguments());
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("JUnit XML report output path is not set.");
                return;
            }

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var testCaseName = settings.Name;
            var time = Time.unscaledTime - s_startTime;
            var testCase = CreateTestCase(testCaseName, time, message, stackTrace, exitCode);
            var testSuite = CreateTestSuite("DeNA.Anjin", s_startDatetime, testCase);
            var testSuites = CreateTestSuites("DeNA.Anjin", testSuite);
            new XDocument(new XDeclaration("1.0", "utf-8", null), testSuites).Save(path);

            await UniTask.CompletedTask;
        }

        internal static string GetOutputPath(string outputPathField, Arguments args = null)
        {
            if (args == null)
            {
                args = new Arguments();
            }

            string path;
            if (args.JUnitReportPath.IsCaptured())
            {
                path = args.JUnitReportPath.Value();
            }
            else if (!string.IsNullOrEmpty(outputPathField))
            {
                path = PathUtils.GetAbsolutePath(outputPathField);
            }
            else
            {
                return null;
            }

            return path;
        }

        internal static XElement CreateTestCase(string name, float time, string message, string stackTrace,
            ExitCode exitCode)
        {
            var element = new XElement("testcase",
                new XAttribute("name", name),
                new XAttribute("classname", "DeNA.Anjin.Autopilot"),
                new XAttribute("time", time),
                new XAttribute("status", exitCode == ExitCode.Normally ? "Passed" : "Failed"));

            switch (exitCode)
            {
                case ExitCode.Normally:
                    break;
                case ExitCode.UnCatchExceptions:
                    element.Add(new XElement("error",
                        new XAttribute("message", message),
                        new XAttribute("type", ""),
                        new XCData(stackTrace)));
                    break;
                case ExitCode.AutopilotFailed:
                case ExitCode.AutopilotLaunchingFailed:
                    element.Add(new XElement("failure",
                        new XAttribute("message", message),
                        new XAttribute("type", ""),
                        new XCData(stackTrace)));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(exitCode), exitCode, null);
            }

            return element;
        }

        internal static XElement CreateTestSuite(string name, DateTime timestamp, XElement testCase)
        {
            var errors = testCase.Element("error") != null ? 1 : 0;
            var failures = testCase.Element("failure") != null ? 1 : 0;
            var time = testCase.Attribute("time")?.Value ?? "0.0";

            var element = new XElement("testsuite",
                new XAttribute("name", name),
                new XAttribute("tests", 1),
                new XAttribute("id", 0),
                new XAttribute("disabled", 0),
                new XAttribute("errors", errors),
                new XAttribute("failures", failures),
                new XAttribute("skipped", 0),
                new XAttribute("time", time),
                new XAttribute("timestamp", timestamp.ToString("yyyy-MM-ddTHH:mm:ss")),
                testCase);

            return element;
        }

        internal static XElement CreateTestSuites(string name, XElement testSuite)
        {
            var errors = testSuite.Attribute("errors")?.Value ?? "0";
            var failures = testSuite.Attribute("failures")?.Value ?? "0";
            var time = testSuite.Attribute("time")?.Value ?? "0.0";

            var element = new XElement("testsuites",
                new XAttribute("name", name),
                new XAttribute("disabled", 0),
                new XAttribute("errors", errors),
                new XAttribute("failures", failures),
                new XAttribute("tests", 1),
                new XAttribute("time", time),
                testSuite);

            return element;
        }
    }
}
