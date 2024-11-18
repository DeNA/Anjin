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
            return sut;
        }

        private static Arguments CreateNotCapturedArguments()
        {
            var arguments = new StubArguments
            {
                _lifespanSec = new StubArgument<int>(), // Not captured
                _randomSeed = new StubArgument<string>(), // Not captured
                _timeScale = new StubArgument<float>(), // Not captured
            };
            return arguments;
        }

        [Test]
        public void OverwriteByCommandLineArguments_HasNotCommandlineArguments_KeepScriptableObjectValues()
        {
            var sut = CreateAutopilotSettings();
            sut.OverwriteByCommandLineArguments(CreateNotCapturedArguments());

            Assert.That(sut.lifespanSec, Is.EqualTo(5));
            Assert.That(sut.randomSeed, Is.EqualTo("1"));
            Assert.That(sut.timeScale, Is.EqualTo(1.0f));
        }

        private static Arguments CreateCapturedArguments()
        {
            var arguments = new StubArguments
            {
                _lifespanSec = new StubArgument<int>(true, 2),
                _randomSeed = new StubArgument<string>(true, ""),
                _timeScale = new StubArgument<float>(true, 2.5f),
            };
            return arguments;
        }

        [Test]
        public void OverwriteByCommandLineArguments_HasCommandlineArguments_OverwriteValues()
        {
            var sut = CreateAutopilotSettings();
            sut.OverwriteByCommandLineArguments(CreateCapturedArguments());

            Assert.That(sut.lifespanSec, Is.EqualTo(2));
            Assert.That(sut.randomSeed, Is.EqualTo(""));
            Assert.That(sut.timeScale, Is.EqualTo(2.5f));
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
            Assert.That(settings.loggerAsset, Is.Null);

            Assert.That(legacyLogger.Logs, Has.Member((LogType.Warning,
                @"Single Logger setting in AutopilotSettings has been obsolete.
Now, automatically converted it to Logger*s*.
Check it out and commit it to your VCS.")));
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
            Assert.That(settings.reporter, Is.Null);

            Assert.That(spyLogger.Logs, Has.Member((LogType.Warning,
                @"Single Reporter setting in AutopilotSettings has been obsolete.
Now, automatically converted it to Reporter*s*.
Check it out and commit it to your VCS.")));
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
            Assert.That(slackReporter.slackToken, Is.EqualTo("token"));
            Assert.That(slackReporter.slackChannels, Is.EqualTo("channels"));
            Assert.That(slackReporter.mentionSubTeamIDs, Is.EqualTo("subteam"));
            Assert.That(slackReporter.addHereInSlackMessage, Is.True);
            Assert.That(settings.slackToken, Is.Null);
            Assert.That(settings.slackChannels, Is.Null);
            Assert.That(settings.mentionSubTeamIDs, Is.Null);
            Assert.That(settings.addHereInSlackMessage, Is.False);

            Assert.That(spyLogger.Logs, Has.Member((LogType.Warning,
                @"Slack settings in AutopilotSettings has been obsolete.
Now, automatically converted it to SlackReporter asset file.
Check it out and commit it to your VCS.")));
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
            Assert.That(settings.observerAgent, Is.Null);

            Assert.That(spyLogger.Logs, Has.Member((LogType.Warning,
                @"ObserverAgent setting in AutopilotSettings has been obsolete.
Now, automatically converted it to SceneCrossingAgents.
Check it out and commit it to your VCS.")));
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

        [Test]
        public void ConvertJUnitXmlReporterFromObsoleteJUnitReportPath_HasJUnitReportPath_GenerateJUnitXmlReporter()
        {
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            settings.junitReportPath = "Path/To/JUnitReport.xml";

            var spyLogger = ScriptableObject.CreateInstance<SpyLoggerAsset>();
            settings.ConvertJUnitXmlReporterFromObsoleteJUnitReportPath(spyLogger.Logger);

            Assert.That(settings.Reporter.reporters.Count, Is.EqualTo(1));
            var reporter = settings.Reporter.reporters[0] as JUnitXmlReporter;
            Assert.That(reporter, Is.Not.Null);
            Assert.That(reporter.outputPath, Is.EqualTo("Path/To/JUnitReport.xml"));
            Assert.That(settings.junitReportPath, Is.Null);

            Assert.That(spyLogger.Logs, Has.Member((LogType.Warning,
                @"JUnitReportPath setting in AutopilotSettings has been obsolete.
Now, automatically converted it to JUnitXmlReporter asset file.
Check it out and commit it to your VCS.")));
        }

        [Test]
        public void ConvertJUnitXmlReporterFromObsoleteJUnitReportPath_HasNotJUnitReportPath_NotGenerateReporter()
        {
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            // Not set junitReportPath

            settings.ConvertJUnitXmlReporterFromObsoleteJUnitReportPath(Debug.unityLogger);
            Assert.That(settings.Reporter.reporters, Is.Empty);
        }

        [Test]
        public void ConvertJUnitXmlReporterFromObsoleteJUnitReportPath_ExistReporter_NotGenerateReporter()
        {
            var existReporter = ScriptableObject.CreateInstance<JUnitXmlReporter>();
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            settings.Reporter.reporters.Add(existReporter); // already exists
            settings.junitReportPath = "Path/To/JUnitReport.xml";

            settings.ConvertJUnitXmlReporterFromObsoleteJUnitReportPath(Debug.unityLogger);
            Assert.That(settings.Reporter.reporters.Count, Is.EqualTo(1));
            Assert.That(settings.Reporter.reporters, Does.Contain(existReporter));
        }

        [TestCase(true, false, false, false)]
        [TestCase(false, true, false, false)]
        [TestCase(false, false, true, false)]
        [TestCase(false, false, false, true)]
        public void ConvertErrorHandlerAgentFromObsoleteSettings_HasAnyHandlingFlag_GenerateErrorHandlerAgent(
            bool handleException,
            bool handleError,
            bool handleAssert,
            bool handleWarning)
        {
            var ignoreMessages = new string[] { "message1", "message2" };
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            settings.handleException = handleException;
            settings.handleError = handleError;
            settings.handleAssert = handleAssert;
            settings.handleWarning = handleWarning;
            settings.ignoreMessages = ignoreMessages;

            var spyLogger = ScriptableObject.CreateInstance<SpyLoggerAsset>();
            settings.ConvertErrorHandlerAgentFromObsoleteSettings(spyLogger.Logger);

            Assert.That(settings.sceneCrossingAgents.Count, Is.EqualTo(1));
            var errorHandlerAgent = settings.sceneCrossingAgents[0] as ErrorHandlerAgent;
            Assert.That(errorHandlerAgent, Is.Not.Null);
            Assert.That(errorHandlerAgent.handleException,
                Is.EqualTo(handleException ? HandlingBehavior.TerminateAutopilot : HandlingBehavior.Ignore));
            Assert.That(errorHandlerAgent.handleError,
                Is.EqualTo(handleError ? HandlingBehavior.TerminateAutopilot : HandlingBehavior.Ignore));
            Assert.That(errorHandlerAgent.handleAssert,
                Is.EqualTo(handleAssert ? HandlingBehavior.TerminateAutopilot : HandlingBehavior.Ignore));
            Assert.That(errorHandlerAgent.handleWarning,
                Is.EqualTo(handleWarning ? HandlingBehavior.TerminateAutopilot : HandlingBehavior.Ignore));
            Assert.That(errorHandlerAgent.ignoreMessages, Is.EqualTo(ignoreMessages));
            Assert.That(settings.handleException, Is.False);
            Assert.That(settings.handleError, Is.False);
            Assert.That(settings.handleAssert, Is.False);
            Assert.That(settings.handleWarning, Is.False);
            Assert.That(settings.ignoreMessages, Is.Null);

            Assert.That(spyLogger.Logs, Has.Member((LogType.Warning,
                @"Error handling settings in AutopilotSettings has been obsolete.
Now, automatically converted it to ErrorHandlerAgent asset file.
Check it out and commit it to your VCS.")));
        }

        [Test]
        public void ConvertErrorHandlerAgentFromObsoleteSettings_HasNotHandlingFlags_NotGenerateErrorHandlerAgent()
        {
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            settings.handleException = false;
            settings.handleError = false;
            settings.handleAssert = false;
            settings.handleWarning = false;

            settings.ConvertErrorHandlerAgentFromObsoleteSettings(Debug.unityLogger);
            Assert.That(settings.sceneCrossingAgents, Is.Empty);
        }

        [Test]
        public void ConvertErrorHandlerAgentFromObsoleteSettings_ExistErrorHandlerAgent_NotGenerateErrorHandlerAgent()
        {
            var existAgent = ScriptableObject.CreateInstance<ErrorHandlerAgent>();
            var settings = ScriptableObject.CreateInstance<AutopilotSettings>();
            settings.sceneCrossingAgents.Add(existAgent); // already exists
            settings.handleException = true;
            settings.handleError = false;
            settings.handleAssert = false;
            settings.handleWarning = false;

            settings.ConvertErrorHandlerAgentFromObsoleteSettings(Debug.unityLogger);
            Assert.That(settings.sceneCrossingAgents.Count, Is.EqualTo(1));
            Assert.That(settings.sceneCrossingAgents, Does.Contain(existAgent));
        }
    }
}
