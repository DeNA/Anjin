// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using DeNA.Anjin.ArgumentCapture;

namespace DeNA.Anjin.Settings
{
    /// <summary>
    /// Commandline arguments
    /// Override .asset settings in batchmode.
    /// </summary>
    public class Arguments
    {
        /// <summary>
        /// Path to the run autopilot settings file (required)
        /// </summary>
        public virtual IArgument<string> AutopilotSettings => new Argument<string>("AUTOPILOT_SETTINGS");

        /// <summary>
        /// Specifies the execution time limit in seconds.
        /// Defaults to 300 seconds, 0 specifies unlimited operation.
        /// </summary>
        public virtual IArgument<int> LifespanSec => new Argument<int>("LIFESPAN_SEC", 300);

        /// <summary>
        /// Specify when you want to fix the seed given to the pseudo-random number generator (optional).
        /// 
        /// This is a setting related to the pseudo-random number generator used by the autopilot.
        /// To fix the seed of the pseudo-random number generator in the game itself, it is necessary to implement this setting on the game title side.
        /// </summary>
        /// <remarks>
        /// Seed is integer value, but is made into a string to represent empty.
        /// </remarks>
        public virtual IArgument<string> RandomSeed => new Argument<string>("RANDOM_SEED");

        /// <summary>
        /// Specifies the Time.timeScale. Default is 1.0
        /// </summary>
        public virtual IArgument<float> TimeScale => new Argument<float>("TIME_SCALE", 1.0f);

        /// <summary>
        /// Specifies the JUnit format report file output path (optional).
        /// 
        /// If there are zero errors and zero failures, the autopilot run is considered to have completed successfully.
        /// </summary>
        public virtual IArgument<string> JUnitReportPath => new Argument<string>("JUNIT_REPORT_PATH");

        /// <summary>
        /// Web API token used for Slack notifications.
        /// 
        /// if omitted, no notifications will be sent.
        /// <see href="https://api.slack.com/web"/>
        /// </summary>
        public virtual IArgument<string> SlackToken => new Argument<string>("SLACK_TOKEN");

        /// <summary>
        /// Channels to send Slack notifications.
        /// 
        /// not notified if omitted. Multiple channels can be specified by separating them with commas.
        /// <see href="https://api.slack.com/web"/>
        /// </summary>
        public virtual IArgument<string> SlackChannels => new Argument<string>("SLACK_CHANNELS");
    }
}
