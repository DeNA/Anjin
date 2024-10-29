// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using DeNA.Anjin.Agents;
using DeNA.Anjin.Attributes;
using DeNA.Anjin.Loggers;
using DeNA.Anjin.Reporters;
using NUnit.Framework;
using UnityEngine;

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
        [ScenePicker("Scene")] public string scenePath;

        /// <summary>
        /// Agent
        /// </summary>
        public AbstractAgent agent;
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
        [Multiline] public string description;
#endif

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
        public AbstractAgent observerAgent;

        /// <summary>
        /// Autopilot running lifespan [sec]. When specified zero, so unlimited running
        /// </summary>
        public int lifespanSec = 300;

        /// <summary>
        /// Random using the specified seed value
        /// </summary>
        public string randomSeed;

        /// <summary>
        /// Time.timeScale
        /// </summary>
        public float timeScale = 1.0f;

        /// <summary>
        /// JUnit report output path
        /// </summary>
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
        public bool handleException = true;

        /// <summary>
        /// Slack notification when Error detected in log
        /// </summary>
        public bool handleError = true;

        /// <summary>
        /// Slack notification when Assert detected in log
        /// </summary>
        public bool handleAssert = true;

        /// <summary>
        /// Slack notification when Warning detected in log
        /// </summary>
        public bool handleWarning;

        /// <summary>
        /// Do not send Slack notifications when log messages contain this string
        /// </summary>
        public string[] ignoreMessages;

        /// <summary>
        /// Logger used for this autopilot settings.
        /// </summary>
        [Obsolete("Use `loggerAssets` field or `LoggerAsset` property instead")]
        public AbstractLoggerAsset loggerAsset;

        /// <summary>
        /// List of Loggers used for this autopilot settings.
        /// </summary>
        public List<AbstractLoggerAsset> loggerAssets;

        /// <summary>
        /// Composite Loggers used for this autopilot settings.
        /// Contains all Loggers of <c>loggerAssets</c> field. Create an instance at launch autopilot.
        /// </summary>
        public CompositeLoggerAsset LoggerAsset { get; private set; }

        /// <summary>
        /// Reporter that called when some errors occurred
        /// </summary>
        [Obsolete("Use `reporters` field  or `Reporter` property instead")]
        public AbstractReporter reporter;

        /// <summary>
        /// List of Reporters to be called on Autopilot terminate.
        /// </summary>
        public List<AbstractReporter> reporters;

        /// <summary>
        /// Composite Reporters to be called on Autopilot terminate.
        /// Contains all Reporters of <c>reporters</c> field. Create an instance at launch autopilot.
        /// </summary>
        public CompositeReporter Reporter { get; private set; }

        /// <summary>
        /// Overwrites specified values in the command line arguments
        /// </summary>
        public void OverrideByCommandLineArguments(Arguments args)
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

            if (args.JUnitReportPath.IsCaptured())
            {
                junitReportPath = args.JUnitReportPath.Value();
            }

            if (args.HandleException.IsCaptured())
            {
                handleException = args.HandleException.Value();
            }

            if (args.HandleError.IsCaptured())
            {
                handleError = args.HandleError.Value();
            }

            if (args.HandleAssert.IsCaptured())
            {
                handleAssert = args.HandleAssert.Value();
            }

            if (args.HandleWarning.IsCaptured())
            {
                handleWarning = args.HandleWarning.Value();
            }
        }

        [InitializeOnLaunchAutopilot]
        private static void Initialize()
        {
            var settings = AutopilotState.Instance.settings;
            Assert.NotNull(settings);
            settings.CreateCompositeLoggerAndReporter();
        }

        private void CreateCompositeLoggerAndReporter()
        {
            this.LoggerAsset = ScriptableObject.CreateInstance<CompositeLoggerAsset>();
            this.LoggerAsset.loggerAssets = loggerAssets;
            this.Reporter = ScriptableObject.CreateInstance<CompositeReporter>();
            this.Reporter.reporters = reporters;
        }
    }
}
