// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using DeNA.Anjin.Agents;
using DeNA.Anjin.Loggers;
using DeNA.Anjin.Reporters;
using DeNA.Anjin.TestDoubles;
using NUnit.Framework;
using UnityEngine;

namespace DeNA.Anjin.Settings
{
    public class AutopilotSettingsTest
    {
        private static AutopilotSettings CreateAutopilotSettings()
        {
            var sut = ScriptableObject.CreateInstance<AutopilotSettings>();
            sut.lifespanSec = 5;
            sut.randomSeed = "1";
            sut.timeScale = 1.0f;
            sut.junitReportPath = "path/to/junit_report.xml";
            sut.handleException = true;
            sut.handleError = true;
            sut.handleAssert = true;
            sut.handleWarning = true;
            return sut;
        }

        private static Arguments CreateNotCapturedArguments()
        {
            var arguments = new StubArguments
            {
                _lifespanSec = new StubArgument<int>(), // Not captured
                _randomSeed = new StubArgument<string>(), // Not captured
                _timeScale = new StubArgument<float>(), // Not captured
                _jUnitReportPath = new StubArgument<string>(), // Not captured
                _handleException = new StubArgument<bool>(), // Not captured
                _handleError = new StubArgument<bool>(), // Not captured
                _handleAssert = new StubArgument<bool>(), // Not captured
                _handleWarning = new StubArgument<bool>(), // Not captured
            };
            return arguments;
        }

        [Test]
        public void OverrideByCommandLineArguments_HasNotCommandlineArguments_KeepScriptableObjectValues()
        {
            var sut = CreateAutopilotSettings();
            sut.OverrideByCommandLineArguments(CreateNotCapturedArguments());

            Assert.That(sut.lifespanSec, Is.EqualTo(5));
            Assert.That(sut.randomSeed, Is.EqualTo("1"));
            Assert.That(sut.timeScale, Is.EqualTo(1.0f));
            Assert.That(sut.junitReportPath, Is.EqualTo("path/to/junit_report.xml"));
            Assert.That(sut.handleException, Is.True);
            Assert.That(sut.handleError, Is.True);
            Assert.That(sut.handleAssert, Is.True);
            Assert.That(sut.handleWarning, Is.True);
        }

        private static Arguments CreateCapturedArguments()
        {
            var arguments = new StubArguments
            {
                _lifespanSec = new StubArgument<int>(true, 2),
                _randomSeed = new StubArgument<string>(true, ""),
                _timeScale = new StubArgument<float>(true, 2.5f),
                _jUnitReportPath = new StubArgument<string>(true, "/path/to/another_junit_report.xml"),
                _handleException = new StubArgument<bool>(true, false),
                _handleError = new StubArgument<bool>(true, false),
                _handleAssert = new StubArgument<bool>(true, false),
                _handleWarning = new StubArgument<bool>(true, false),
            };
            return arguments;
        }

        [Test]
        public void OverrideByCommandLineArguments_HasCommandlineArguments_OverwriteValues()
        {
            var sut = CreateAutopilotSettings();
            sut.OverrideByCommandLineArguments(CreateCapturedArguments());

            Assert.That(sut.lifespanSec, Is.EqualTo(2));
            Assert.That(sut.randomSeed, Is.EqualTo(""));
            Assert.That(sut.timeScale, Is.EqualTo(2.5f));
            Assert.That(sut.junitReportPath, Is.EqualTo("/path/to/another_junit_report.xml"));
            Assert.That(sut.handleException, Is.False);
            Assert.That(sut.handleError, Is.False);
            Assert.That(sut.handleAssert, Is.False);
            Assert.That(sut.handleWarning, Is.False);
        }

        [Test]
        public void ConvertLoggersFromObsoleteLogger_HasLogger_IncludeToLoggers()
        {
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            var legacyLogger = ScriptableObject.CreateInstance<SpyLoggerAsset>();
            settings.loggerAsset = legacyLogger; // already exists

            settings.ConvertLoggersFromObsoleteLogger();
            Assert.That(settings.loggerAssets.Count, Is.EqualTo(1));
            Assert.That(settings.loggerAssets, Has.Member(legacyLogger));

            Assert.That(legacyLogger.Logs, Has.Member((LogType.Warning,
                @"Single Logger setting in AutopilotSettings has been obsolete.
Please delete the reference using Debug Mode in the Inspector window. And add to the list Loggers.
This time, temporarily converting.")));
        }

        [Test]
        public void ConvertLoggersFromObsoleteLogger_HasNotLogger_NotIncludeToLoggers()
        {
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            // Not set (single) logger

            settings.ConvertLoggersFromObsoleteLogger();
            Assert.That(settings.LoggerAsset.loggerAssets, Is.Empty);
        }

        [Test]
        public void ConvertLoggersFromObsoleteLogger_ExistLogger_NotIncludeToLoggers()
        {
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            settings.loggerAssets.Add(ScriptableObject.CreateInstance<CompositeLoggerAsset>()); // already exists
            settings.loggerAsset = ScriptableObject.CreateInstance<ConsoleLoggerAsset>();

            settings.ConvertLoggersFromObsoleteLogger();
            Assert.That(settings.LoggerAsset.loggerAssets.Count, Is.EqualTo(1));
            Assert.That(settings.LoggerAsset.loggerAssets, Has.No.InstanceOf<ConsoleLoggerAsset>());
        }

        [Test]
        public void ConvertReportersFromObsoleteReporter_HasReporter_IncludeToReporters()
        {
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            var legacyReporter = ScriptableObject.CreateInstance<SlackReporter>();
            settings.reporter = legacyReporter;

            var spyLogger = ScriptableObject.CreateInstance<SpyLoggerAsset>();
            settings.ConvertReportersFromObsoleteReporter(spyLogger.Logger);
            Assert.That(settings.reporters.Count, Is.EqualTo(1));
            Assert.That(settings.reporters, Has.Member(legacyReporter));

            Assert.That(spyLogger.Logs, Has.Member((LogType.Warning,
                @"Single Reporter setting in AutopilotSettings has been obsolete.
Please delete the reference using Debug Mode in the Inspector window. And add to the list Reporters.
This time, temporarily converting.")));
        }

        [Test]
        public void ConvertReportersFromObsoleteReporter_HasNotReporter_NotIncludeToReporters()
        {
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            // Not set (single) reporter

            settings.ConvertReportersFromObsoleteReporter(Debug.unityLogger);
            Assert.That(settings.Reporter.reporters, Is.Empty);
        }

        [Test]
        public void ConvertReportersFromObsoleteReporter_ExistReporter_NotIncludeToReporters()
        {
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            settings.reporters.Add(ScriptableObject.CreateInstance<CompositeReporter>()); // already exists
            settings.reporter = ScriptableObject.CreateInstance<SlackReporter>();

            settings.ConvertReportersFromObsoleteReporter(Debug.unityLogger);
            Assert.That(settings.Reporter.reporters.Count, Is.EqualTo(1));
            Assert.That(settings.Reporter.reporters, Has.No.InstanceOf<SlackReporter>());
        }

        [Test]
        public void ConvertSlackReporterFromObsoleteSlackSettings_HasSlackSettings_GenerateSlackReporter()
        {
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            settings.slackToken = "token";
            settings.slackChannels = "channels";
            settings.mentionSubTeamIDs = "subteam";
            settings.addHereInSlackMessage = true;

            var spyLogger = ScriptableObject.CreateInstance<SpyLoggerAsset>();
            settings.ConvertSlackReporterFromObsoleteSlackSettings(spyLogger.Logger);

            Assert.That(settings.reporters.Count, Is.EqualTo(1));
            var slackReporter = settings.reporters[0] as SlackReporter;
            Assert.That(slackReporter, Is.Not.Null);
            Assert.That(slackReporter.slackToken, Is.EqualTo(settings.slackToken));
            Assert.That(slackReporter.slackChannels, Is.EqualTo(settings.slackChannels));
            Assert.That(slackReporter.mentionSubTeamIDs, Is.EqualTo(settings.mentionSubTeamIDs));
            Assert.That(slackReporter.addHereInSlackMessage, Is.EqualTo(settings.addHereInSlackMessage));

            Assert.That(spyLogger.Logs, Has.Member((LogType.Warning,
                @"Slack settings in AutopilotSettings has been obsolete.
Please delete the value using Debug Mode in the Inspector window. And create a SlackReporter asset file.
This time, temporarily generate and use SlackReporter instance.")));
        }

        [Test]
        public void ConvertSlackReporterFromObsoleteSlackSettings_HasNotSlackToken_NotGenerateSlackReporter()
        {
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            settings.slackToken = "token";
            // Not set slackChannels

            settings.ConvertSlackReporterFromObsoleteSlackSettings(Debug.unityLogger);
            Assert.That(settings.reporters, Is.Empty);
        }

        [Test]
        public void ConvertSlackReporterFromObsoleteSlackSettings_HasNotSlackChannels_NotGenerateSlackReporter()
        {
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            settings.slackChannels = "channels";
            // Not set slackToken

            settings.ConvertSlackReporterFromObsoleteSlackSettings(Debug.unityLogger);
            Assert.That(settings.reporters, Is.Empty);
        }

        [Test]
        public void ConvertSlackReporterFromObsoleteSlackSettings_ExistReporter_NotGenerateSlackReporter()
        {
            var existReporter = ScriptableObject.CreateInstance<SlackReporter>();
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            settings.reporters.Add(existReporter); // already exists
            settings.slackToken = "token";
            settings.slackChannels = "channels";

            settings.ConvertSlackReporterFromObsoleteSlackSettings(Debug.unityLogger);
            Assert.That(settings.reporters.Count, Is.EqualTo(1));
            Assert.That(settings.reporters, Does.Contain(existReporter));
        }

        [Test]
        public void ConvertSceneCrossingAgentsFromObsoleteObserverAgent_HasObserverAgent_IncludeToCrossSceneAgents()
        {
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            var legacyObserverAgent = ScriptableObject.CreateInstance<SpyAgent>();
            settings.observerAgent = legacyObserverAgent;

            var spyLogger = ScriptableObject.CreateInstance<SpyLoggerAsset>();
            settings.ConvertSceneCrossingAgentsFromObsoleteObserverAgent(spyLogger.Logger);
            Assert.That(settings.sceneCrossingAgents.Count, Is.EqualTo(1));
            Assert.That(settings.sceneCrossingAgents, Has.Member(legacyObserverAgent));

            Assert.That(spyLogger.Logs, Has.Member((LogType.Warning,
                @"ObserverAgent setting in AutopilotSettings has been obsolete.
Please delete the value using Debug Mode in the Inspector window. And using the SceneCrossingAgents.
This time, temporarily converting.")));
        }

        [Test]
        public void
            ConvertSceneCrossingAgentsFromObsoleteObserverAgent_HasNotObserverAgent_NotIncludeToCrossSceneAgents()
        {
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            // Not set observerAgent

            settings.ConvertSceneCrossingAgentsFromObsoleteObserverAgent(Debug.unityLogger);
            Assert.That(settings.sceneCrossingAgents, Is.Empty);
        }

        [Test]
        public void ConvertSceneCrossingAgentsFromObsoleteObserverAgent_ExistAgent_NotIncludeToCrossSceneAgents()
        {
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            settings.sceneCrossingAgents.Add(ScriptableObject.CreateInstance<DoNothingAgent>()); // already exists
            settings.observerAgent = ScriptableObject.CreateInstance<SpyAgent>();

            settings.ConvertSceneCrossingAgentsFromObsoleteObserverAgent(Debug.unityLogger);
            Assert.That(settings.sceneCrossingAgents.Count, Is.EqualTo(1));
            Assert.That(settings.sceneCrossingAgents, Has.No.InstanceOf<SpyAgent>());
        }
    }
}
