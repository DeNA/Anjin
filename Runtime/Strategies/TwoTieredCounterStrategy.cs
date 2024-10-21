// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DeNA.Anjin.Attributes;
using TestHelper.Monkey.ScreenshotFilenameStrategies;

namespace DeNA.Anjin.Strategies
{
    /// <summary>
    /// Screenshot filename strategy that adds a unique counter to the prefix.
    /// </summary>
    public class TwoTieredCounterStrategy : AbstractPrefixAndUniqueIDStrategy
    {
        private static readonly Dictionary<string, int> s_prefixCounters = new Dictionary<string, int>();
        private readonly int _uniqueNumberOfPrefix;
        private int _count;

        [InitializeOnLaunchAutopilot]
        internal static void ResetPrefixCounters()
        {
            s_prefixCounters.Clear();
        }

        /// <inheritdoc/>
        public TwoTieredCounterStrategy(string filenamePrefix = null, [CallerMemberName] string callerMemberName = null)
            : base(filenamePrefix, callerMemberName)
        {
            var key = base.GetFilenamePrefix();
            if (!s_prefixCounters.TryAdd(key, 1))
            {
                s_prefixCounters[key]++;
            }

            _uniqueNumberOfPrefix = s_prefixCounters[key];
            _count = 0;
        }

        /// <inheritdoc/>
        protected override string GetFilenamePrefix()
        {
            return $"{base.GetFilenamePrefix()}{_uniqueNumberOfPrefix:D2}";
        }

        /// <inheritdoc/>
        protected override string GetUniqueID()
        {
            return $"{++_count:D4}";
        }
    }
}
