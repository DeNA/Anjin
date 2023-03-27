// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using DeNA.Anjin.ArgumentCapture;
using DeNA.Anjin.Settings;

namespace DeNA.Anjin.TestDoubles
{
    public class StubArguments : Arguments
    {
        internal IArgument<string> _autopilotSettings;
        internal IArgument<int> _lifespanSec;
        internal IArgument<string> _randomSeed;
        internal IArgument<float> _timeScale;
        internal IArgument<string> _jUnitReportPath;
        internal IArgument<string> _slackToken;
        internal IArgument<string> _slackChannels;

        public override IArgument<string> AutopilotSettings
        {
            get
            {
                return _autopilotSettings;
            }
        }

        public override IArgument<int> LifespanSec
        {
            get
            {
                return _lifespanSec;
            }
        }

        public override IArgument<string> RandomSeed
        {
            get
            {
                return _randomSeed;
            }
        }

        public override IArgument<float> TimeScale
        {
            get
            {
                return _timeScale;
            }
        }

        public override IArgument<string> JUnitReportPath
        {
            get
            {
                return _jUnitReportPath;
            }
        }

        public override IArgument<string> SlackToken
        {
            get
            {
                return _slackToken;
            }
        }

        public override IArgument<string> SlackChannels
        {
            get
            {
                return _slackChannels;
            }
        }
    }
}
