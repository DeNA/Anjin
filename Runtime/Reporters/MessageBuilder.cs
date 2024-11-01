// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

namespace DeNA.Anjin.Reporters
{
    /// <summary>
    /// Message builder for Reporters.
    ///
    /// Placeholder:
    ///     - {message}: Message with terminate (e.g., error log message)
    /// </summary>
    public static class MessageBuilder
    {
        /// <summary>
        /// Returns messages that replaced placeholders in the template.
        /// </summary>
        /// <param name="template">Message body template</param>
        /// <param name="message">Replace placeholder "{message}" in the template with this string</param>
        /// <returns>Messages that replaced placeholders in the template</returns>
        public static string BuildWithTemplate(string template, string message)
        {
            return template
                .Replace("{message}", message);

            // TODO: Add more placeholders
        }
    }
}
