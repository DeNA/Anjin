// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using DeNA.Anjin.Agents;
using DeNA.Anjin.Attributes;
using DeNA.Anjin.Loggers;
using DeNA.Anjin.Reporters;
using DeNA.Anjin.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DeNA.Anjin.Settings
{
    /// <summary>
    /// Scene x Agent mapping
    /// </summary>
    [Serializable]
    public struct SceneAgentMap
    {
        /// <summary>
        /// Scene
        /// </summary>
        [ScenePicker("Scene")]
        public string scenePath;

        /// <summary>
        /// Agent
        /// </summary>
        public AbstractAgent agent;
    }

    /// <summary>
    /// Autopilot exit code options when lifespan expired.
    /// </summary>
    public enum ExitCodeWhenLifespanExpired
    {
        /// <summary>
        /// Normally terminated.
        /// </summary>
        Normally = ExitCode.Normally,

        /// <summary>
        /// Terminated by Autopilot lifespan expired.
        /// By default, the exit code at expiration will be <c>Normally</c>.
        /// This exit code is only used if set by the user.
        /// </summary>
        LifespanExpired = ExitCode.AutopilotLifespanExpired,

        /// <summary>
        /// Input custom exit code by integer value.
        /// </summary>
        Custom = 1024,
    }

    /// <summary>
    /// Autopilot run settings.
    /// 
    /// Can overridden by startup arguments, except for Agent settings.
    /// </summary>
    [CreateAssetMenu(fileName = "New AutopilotSettings", menuName = "Anjin/Autopilot Settings", order = int.MinValue)]
    public class AutopilotSettings : ScriptableObject
    {
#if UNITY_EDITOR
        /// <summary>
        /// Description about this setting instance
        /// </summary>
        [Multiline]
        public string description;
#endif

        /// <summary>
        /// Custom name of this setting used by Reporter.
        /// When using it from within code, use the <code>Name</code> property.
        /// </summary>
        public new string name;

        /// <summary>
        /// Name of this setting used by Reporter.
        /// If omitted, the asset file name is used.
        /// </summary>
        [SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs")]
        public string Name => string.IsNullOrEmpty(this.name) ? base.name : this.name;

        /// <summary>
        /// Scene to Agent assign mapping
        /// </summary>
        public List<SceneAgentMap> sceneAgentMaps = new List<SceneAgentMap>();

        /// <summary>
        /// Agent for when not assigned
        /// </summary>
        public AbstractAgent fallbackAgent;

        /// <summary>
        /// Parallel running agent using on every scene, for using observer (e.g., EmergencyExitAgent)
        /// </summary>
        [Obsolete("Use crossSceneAgents instead")]
        public AbstractAgent observerAgent;

        /// <summary>
        /// Agents running by scene crossing.
        /// The specified agents will have the same lifespan as <c>Autopilot</c> (i.e., use <c>DontDestroyOnLoad</c>)
        /// for specifying, e.g., <c>ErrorHandlerAgent</c>, <c>UGUIEmergencyExitAgent</c>.
        /// </summary>
        public List<AbstractAgent> sceneCrossingAgents = new List<AbstractAgent>();

        /// <summary>
        /// Autopilot running lifespan [sec]. When specified zero, so unlimited running.
        /// </summary>
        public int lifespanSec = 300;

        /// <summary>
        /// Autopilot exit code options when lifespan expired.
        /// When using it from within code, use the <code>ExitCode</code> property.
        /// </summary>
        public ExitCodeWhenLifespanExpired exitCode = ExitCodeWhenLifespanExpired.Normally;

        /// <summary>
        /// Custom exit code to be used if <code>Custom</code> is selected for <c>exitCode</c>.
        /// Please enter an integer value.
        /// </summary>
        public string customExitCode;

        [SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs")]
        public ExitCode ExitCode
        {
            get
            {
                if (exitCode == ExitCodeWhenLifespanExpired.Custom)
                {
                    return int.TryParse(customExitCode, out var intExitCode)
                        ? (ExitCode)intExitCode
                        : (ExitCode)exitCode;
                }

                return (ExitCode)exitCode;
            }
        }

        /// <summary>
        /// Message used by Reporter.
        /// </summary>
        [Multiline]
        public string exitMessage = "Autopilot lifespan expired.";

        /// <summary>
        /// Random using the specified seed value.
        /// Please enter an integer value or empty.
        /// </summary>
        public string randomSeed;

        /// <summary>
        /// Time.timeScale.
        /// </summary>
        public float timeScale = 1.0f;

        /// <summary>
        /// Output files root directory path used by Agents, Loggers, and Reporters.
        /// When a relative path is specified, the origin is the project root in the Editor, and <c>Application.persistentDataPath</c> on the Player.
        /// This item can be overridden by the command line argument "-OUTPUT_ROOT_DIRECTORY_PATH".
        /// When using it from within code, use the <code>OutputRootPath</code> property.
        /// </summary>
        public string outputRootPath;

        /// <summary>
        /// Output files root directory path used by Agents, Loggers, and Reporters.
        /// This property is returns absolute path.
        /// </summary>
        [SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs")]
        public string OutputRootPath => Application.isEditor
            ? Path.GetFullPath(!string.IsNullOrEmpty(this.outputRootPath) ? this.outputRootPath : ".")
            : PathUtils.GetAbsolutePath(this.outputRootPath, Application.persistentDataPath);

        /// <summary>
        /// Screenshots output directory path used by Agents.
        /// When a relative path is specified, relative to the <c>outputRootPath</c>.
        /// This item can be overridden by the command line argument "-SCREENSHOTS_DIRECTORY_PATH".
        /// When using it from within code, use the <code>ScreenshotsPath</code> property.
        /// </summary>
        public string screenshotsPath = "Screenshots";

        /// <summary>
        /// Screenshots output directory path used by Agents.
        /// This property is returns absolute path.
        /// </summary>
        [SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs")]
        public string ScreenshotsPath => PathUtils.GetAbsolutePath(this.screenshotsPath, this.OutputRootPath);

        /// <summary>
        /// Returns whether the <c>screenshotsPath</c> is specified.
        /// </summary>
        [SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs")]
        public bool IsSpecifiedScreenshotsPath => !string.IsNullOrEmpty(this.screenshotsPath);

        /// <summary>
        /// Clean screenshots under <c>screenshotsPath</c> when launching Autopilot.
        /// </summary>
        public bool cleanScreenshots;

        /// <summary>
        /// JUnit report output path
        /// </summary>
        [Obsolete("Use JUnitXmlReporter instead")]
        public string junitReportPath;

        /// <summary>
        /// Slack API token
        /// </summary>
        [Obsolete("this option moved to SlackReporter")]
        public string slackToken;

        /// <summary>
        /// Slack channels to send notification
        /// </summary>
        [Obsolete("this option moved to SlackReporter")]
        public string slackChannels;

        /// <summary>
        /// Mention to sub team ID (comma separates)
        /// </summary>
        [Obsolete("this option moved to SlackReporter")]
        public string mentionSubTeamIDs;

        /// <summary>
        /// Add @here
        /// </summary>
        [Obsolete("this option moved to SlackReporter")]
        public bool addHereInSlackMessage;

        /// <summary>
        /// Slack notification when Exception detected in log
        /// </summary>
        [Obsolete("Use ErrorHandlerAgent instead")]
        public bool handleException;

        /// <summary>
        /// Slack notification when Error detected in log
        /// </summary>
        [Obsolete("Use ErrorHandlerAgent instead")]
        public bool handleError;

        /// <summary>
        /// Slack notification when Assert detected in log
        /// </summary>
        [Obsolete("Use ErrorHandlerAgent instead")]
        public bool handleAssert;

        /// <summary>
        /// Slack notification when Warning detected in log
        /// </summary>
        [Obsolete("Use ErrorHandlerAgent instead")]
        public bool handleWarning;

        /// <summary>
        /// Do not send Slack notifications when log messages contain this string
        /// </summary>
        [Obsolete("Use ErrorHandlerAgent instead")]
        public string[] ignoreMessages;

        /// <summary>
        /// Logger used for this autopilot settings.
        /// </summary>
        [Obsolete("Use `LoggerAsset` property instead")]
        public AbstractLoggerAsset loggerAsset;

        /// <summary>
        /// List of Loggers used for this autopilot settings.
        /// </summary>
        public List<AbstractLoggerAsset> loggerAssets = new List<AbstractLoggerAsset>();

        private CompositeLoggerAsset _compositeLoggerAsset;

        /// <summary>
        /// Composite Loggers used for this autopilot settings.
        /// Contains <c>loggerAssets</c> field. Create an instance during the first call.
        /// </summary>
        public CompositeLoggerAsset LoggerAsset
        {
            get
            {
                if (_compositeLoggerAsset == null)
                {
                    _compositeLoggerAsset = CreateInstance<CompositeLoggerAsset>();
                    _compositeLoggerAsset.loggerAssets = this.loggerAssets;
                }

                return _compositeLoggerAsset;
            }
        }

        /// <summary>
        /// Reporter that called when some errors occurred
        /// </summary>
        [Obsolete("Use `Reporter` property instead")]
        public AbstractReporter reporter;

        /// <summary>
        /// List of Reporters to be called on Autopilot terminate.
        /// </summary>
        public List<AbstractReporter> reporters = new List<AbstractReporter>();

        private CompositeReporter _compositeReporter;

        /// <summary>
        /// Composite Reporters to be called on Autopilot terminate.
        /// Contains <c>reporters</c> field. Create an instance during the first call.
        /// </summary>
        public CompositeReporter Reporter
        {
            get
            {
                if (_compositeReporter == null)
                {
                    _compositeReporter = CreateInstance<CompositeReporter>();
                    _compositeReporter.reporters = this.reporters;
                }

                return _compositeReporter;
            }
        }

        /// <summary>
        /// Overwrites specified values in the command line arguments
        /// </summary>
        [SuppressMessage("ApiDesign", "RS0030:Do not use banned APIs")]
        internal void OverwriteByCommandLineArguments(Arguments args)
        {
            if (args.RandomSeed.IsCaptured())
            {
                randomSeed = args.RandomSeed.Value();
            }

            if (args.LifespanSec.IsCaptured())
            {
                lifespanSec = args.LifespanSec.Value();
            }

            if (args.TimeScale.IsCaptured())
            {
                timeScale = args.TimeScale.Value();
            }

            if (args.OutputRootPath.IsCaptured())
            {
                outputRootPath = args.OutputRootPath.Value();
            }

            if (args.ScreenshotsPath.IsCaptured())
            {
                screenshotsPath = args.ScreenshotsPath.Value();
            }
        }

        internal void InitializeOutputDirectories()
        {
            var outputRootDirectory = this.OutputRootPath;
            if (!Directory.Exists(outputRootDirectory))
            {
                Directory.CreateDirectory(outputRootDirectory);
            }

            var screenshotsDirectory = this.ScreenshotsPath;
            if (this.cleanScreenshots && IsSpecifiedScreenshotsPath && Directory.Exists(screenshotsDirectory))
            {
                Directory.Delete(screenshotsDirectory, true);
            }

            if (!Directory.Exists(screenshotsDirectory))
            {
                Directory.CreateDirectory(screenshotsDirectory);
            }
        }

        [InitializeOnLaunchAutopilot(InitializeOnLaunchAutopilotAttribute.InitializeSettings)]
        private static void Initialize()
        {
            var settings = AutopilotState.Instance.settings;
            if (settings == null)
            {
                throw new InvalidOperationException("Autopilot is not running");
            }

            settings.OverwriteByCommandLineArguments(new Arguments());
            settings.InitializeOutputDirectories();

            settings.ConvertLoggersFromObsoleteLogger(); // Note: before create default logger.
            settings.CreateDefaultLoggerIfNeeded();

            var logger = settings.LoggerAsset.Logger;
            settings.ConvertReportersFromObsoleteReporter(logger); // Note: before convert other Reporters.
            settings.ConvertSlackReporterFromObsoleteSlackSettings(logger);
            settings.ConvertJUnitXmlReporterFromObsoleteJUnitReportPath(logger);

            settings.ConvertSceneCrossingAgentsFromObsoleteObserverAgent(logger); // Note: before convert other Agents.
            settings.ConvertErrorHandlerAgentFromObsoleteSettings(logger);

#if UNITY_EDITOR && UNITY_2020_3_OR_NEWER
            AssetDatabase.SaveAssetIfDirty(settings);
#endif
        }

        private void CreateDefaultLoggerIfNeeded()
        {
            if (!this.LoggerAsset.loggerAssets.Any()) // no logger settings.
            {
                this.LoggerAsset.loggerAssets = new List<AbstractLoggerAsset> { CreateInstance<ConsoleLoggerAsset>() };
                // not change field directly.

                this.LoggerAsset.Logger.Log("Create default logger.");
            }
        }

        [Obsolete("Remove this method when bump major version")]
        internal void ConvertLoggersFromObsoleteLogger()
        {
            if (this.loggerAsset == null || this.loggerAssets.Any())
            {
                return;
            }

            this.loggerAsset.Logger.Log(LogType.Warning, @"Single Logger setting in AutopilotSettings has been obsolete.
Now, automatically converted it to Logger*s*.
Check it out and commit it to your VCS.");

            this.loggerAssets.Add(this.loggerAsset);
            this.loggerAsset = null;
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        [Obsolete("Remove this method when bump major version")]
        internal void ConvertReportersFromObsoleteReporter(ILogger logger)
        {
            if (this.reporter == null || this.reporters.Any())
            {
                return;
            }

            logger.Log(LogType.Warning, @"Single Reporter setting in AutopilotSettings has been obsolete.
Now, automatically converted it to Reporter*s*.
Check it out and commit it to your VCS.");

            this.reporters.Add(this.reporter);
            this.reporter = null;
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        [Obsolete("Remove this method when bump major version")]
        internal void ConvertSlackReporterFromObsoleteSlackSettings(ILogger logger)
        {
            if (string.IsNullOrEmpty(this.slackToken) || string.IsNullOrEmpty(this.slackChannels) ||
                this.reporters.Any(x => x.GetType() == typeof(SlackReporter)))
            {
                return;
            }

            const string AutoConvertingMessage = @"Slack settings in AutopilotSettings has been obsolete.
Now, automatically converted it to SlackReporter asset file.
Check it out and commit it to your VCS.";
            logger.Log(LogType.Warning, AutoConvertingMessage);

            var convertedReporter = CreateInstance<SlackReporter>();
            convertedReporter.slackToken = this.slackToken;
            convertedReporter.slackChannels = this.slackChannels;
            convertedReporter.mentionSubTeamIDs = this.mentionSubTeamIDs;
            convertedReporter.addHereInSlackMessage = this.addHereInSlackMessage;
#if UNITY_EDITOR
            convertedReporter.description = AutoConvertingMessage;
            SaveConvertedObject(convertedReporter);
#endif

            this.reporters.Add(convertedReporter);
            this.slackToken = null;
            this.slackChannels = null;
            this.mentionSubTeamIDs = null;
            this.addHereInSlackMessage = false;
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        [Obsolete("Remove this method when bump major version")]
        internal void ConvertJUnitXmlReporterFromObsoleteJUnitReportPath(ILogger logger)
        {
            if (string.IsNullOrEmpty(this.junitReportPath) ||
                this.reporters.Any(x => x.GetType() == typeof(JUnitXmlReporter)))
            {
                return;
            }

            const string AutoConvertingMessage = @"JUnitReportPath setting in AutopilotSettings has been obsolete.
Now, automatically converted it to JUnitXmlReporter asset file.
Check it out and commit it to your VCS.";
            logger.Log(LogType.Warning, AutoConvertingMessage);

            var convertedReporter = CreateInstance<JUnitXmlReporter>();
            convertedReporter.outputPath = this.junitReportPath;
#if UNITY_EDITOR
            convertedReporter.description = AutoConvertingMessage;
            SaveConvertedObject(convertedReporter);
#endif

            this.Reporter.reporters.Add(convertedReporter);
            this.junitReportPath = null;
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

#if UNITY_EDITOR
        private void SaveConvertedObject(Object obj)
        {
            var settingsPath = AssetDatabase.GetAssetPath(this);
            if (string.IsNullOrEmpty(settingsPath))
            {
                return;
            }

            var dir = Path.GetDirectoryName(settingsPath) ?? "Assets";
            AssetDatabase.CreateAsset(obj, Path.Combine(dir, $"New {obj.GetType().Name}.asset"));
        }
#endif

        [Obsolete("Remove this method when bump major version")]
        internal void ConvertSceneCrossingAgentsFromObsoleteObserverAgent(ILogger logger)
        {
            if (this.observerAgent == null || this.sceneCrossingAgents.Any())
            {
                return;
            }

            logger.Log(LogType.Warning, @"ObserverAgent setting in AutopilotSettings has been obsolete.
Now, automatically converted it to SceneCrossingAgents.
Check it out and commit it to your VCS.");

            this.sceneCrossingAgents.Add(this.observerAgent);
            this.observerAgent = null;
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        [Obsolete("Remove this method when bump major version")]
        internal void ConvertErrorHandlerAgentFromObsoleteSettings(ILogger logger)
        {
            if ((!this.handleException && !this.handleError && !this.handleAssert && !this.handleWarning) ||
                this.sceneCrossingAgents.Any(x => x.GetType() == typeof(ErrorHandlerAgent)))
            {
                return;
                // Note:
                //  If all are off, no conversion will occur.
                //  No conversion will be performed when creating a new AutopilotSettings because all default values are false.
            }

            const string AutoConvertingMessage = @"Error handling settings in AutopilotSettings has been obsolete.
Now, automatically converted it to ErrorHandlerAgent asset file.
Check it out and commit it to your VCS.";
            logger.Log(LogType.Warning, AutoConvertingMessage);

            var convertedAgent = CreateInstance<ErrorHandlerAgent>();
            convertedAgent.handleException = ErrorHandlerAgent.HandlingBehaviorFrom(this.handleException);
            convertedAgent.handleError = ErrorHandlerAgent.HandlingBehaviorFrom(this.handleError);
            convertedAgent.handleAssert = ErrorHandlerAgent.HandlingBehaviorFrom(this.handleAssert);
            convertedAgent.handleWarning = ErrorHandlerAgent.HandlingBehaviorFrom(this.handleWarning);
            convertedAgent.ignoreMessages = this.ignoreMessages;
#if UNITY_EDITOR
            convertedAgent.description = AutoConvertingMessage;
            SaveConvertedObject(convertedAgent);
#endif

            this.sceneCrossingAgents.Add(convertedAgent);
            this.handleException = false;
            this.handleError = false;
            this.handleAssert = false;
            this.handleWarning = false;
            this.ignoreMessages = null;
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
    }
}
