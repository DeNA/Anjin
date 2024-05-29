// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.IO;

namespace DeNA.Anjin.Utilities
{
    internal static class JUnitReporter
    {
        /// <summary>
        /// Output JUnit XML report format file
        /// </summary>
        /// <param name="path">Output file path</param>
        /// <param name="exitCode">0: success, 1: error, 2 or larger: failure</param>
        /// <param name="logString">Log message when stop by log</param>
        /// <param name="stackTrace">Stack trace when stop by log</param>
        /// <param name="time">Autopilot running time [sec]</param>
        internal static void Output(string path, int exitCode, string logString = null, string stackTrace = null,
            float time = 0)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var xml = CreateJUnitXmlReport(exitCode, logString, stackTrace, time);
            File.WriteAllText(path, xml);
        }

        private static string CreateJUnitXmlReport(int exitCode, string logString, string stackTrace, float time)
        {
            string errors, failures, errorOrFailureElement;
            switch (exitCode)
            {
                case 0:
                    errors = "0";
                    failures = "0";
                    errorOrFailureElement = "";
                    break;
                case 1:
                    errors = "1";
                    failures = "0";
                    errorOrFailureElement = string.Format(ErrorElement, logString, stackTrace);
                    break;
                default:
                    errors = "0";
                    failures = "1";
                    errorOrFailureElement = string.Format(FailureElement, logString, stackTrace);
                    break;
            }

            return string.Format(JUnitXmlReportFormat, errors, failures, time.ToString("F3"), errorOrFailureElement);
        }

        private const string JUnitXmlReportFormat = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<testsuites name=""Anjin""
            disabled=""0""
            errors=""{0}""
            failures=""{1}""
            tests=""1""
            time=""{2}""
            >
            <testsuite  name=""DeNA.Anjin""
                        tests=""1""
                        id=""0""
                        >
                        <testcase   name=""Autopilot""
                                    classname=""DeNA.Anjin""
                                    >{3}
                        </testcase>
            </testsuite>
</testsuites>";

        private const string ErrorElement = @"
                                    <error message=""{0}""
                                           type=""""
                                            ><![CDATA[{1}]]></error>";

        private const string FailureElement = @"
                                    <failure message=""{0}""
                                             type=""""
                                            ><![CDATA[{1}]]></failure>";

        // XML format from: https://github.com/jenkinsci/benchmark-plugin/blob/master/doc/JUnit%20format/JUnit.txt
    }
}
