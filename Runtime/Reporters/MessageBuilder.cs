// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using DeNA.Anjin.Settings;

namespace DeNA.Anjin.Reporters
{
    /// <summary>
    /// Message builder for Reporters.
    /// </summary>
    public static class MessageBuilder
    {
        /// <summary>
        /// Returns messages that replaced placeholders in the template.
        /// </summary>
        /// <param name="template">Message body template.
        /// You can specify placeholders:
        ///     - {message}: Message with terminate (e.g., error log message)
        ///     - {settings}: Name of running AutopilotSettings
        ///     - {env.KEY}: Environment variable
        /// </param>
        /// <param name="message">Replace placeholder "{message}" in the template with this string</param>
        /// <returns>Messages that replaced placeholders in the template</returns>
        public static string BuildWithTemplate(string template, string message)
        {
            var settings = AutopilotState.Instance.settings;
            var placeholders = GetPlaceholders().ToList();
            placeholders.Add(("{message}", message));
            placeholders.Add(("{settings}", settings ? settings.Name : "null"));

            var replacedMessage = template;
            placeholders.ForEach(placeholder =>
            {
                replacedMessage = replacedMessage.Replace(placeholder.placeholder, placeholder.replacement);
            });

            return replacedMessage;
        }

        private static IEnumerable<(string placeholder, string replacement)> GetPlaceholders()
        {
            var env = Environment.GetEnvironmentVariables();
            foreach (var key in env.Keys)
            {
                yield return ("{env." + key + "}", env[key] as string);
            }
        }
    }
}
