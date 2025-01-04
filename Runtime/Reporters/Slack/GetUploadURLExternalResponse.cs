// Copyright (c) 2023-2025 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Text.RegularExpressions;

namespace DeNA.Anjin.Reporters.Slack
{
    /// <summary>
    /// Response of files.getUploadURLExternal.
    /// </summary>
    /// <see href="https://api.slack.com/methods/files.getUploadURLExternal"/>
    public struct GetUploadURLExternalResponse
    {
        public bool Success { get; private set; }

        public string UploadUrl { get; private set; }

        public string FileId { get; private set; }

        private static readonly Regex s_successRegex = new Regex("\"ok\":true");
        private static readonly Regex s_urlUploadRegex = new Regex("\"upload_url\":\"(https?://[\\w\\./-]+)\"");
        private static readonly Regex s_fileIdRegex = new Regex("\"file_id\":\"(\\w+)\"");

        /// <summary>
        /// Constructor.
        /// </summary>
        public GetUploadURLExternalResponse(bool success = false)
        {
            Success = success;
            UploadUrl = null;
            FileId = null;
        }

        /// <summary>
        /// Parse REST API response body and set properties.
        /// </summary>
        /// <param name="text">REST API response body</param>
        public bool ParseResponse(string text)
        {
            Success = s_successRegex.Match(text).Success;
            if (!Success)
            {
                return false;
            }

            var uploadUrlMatch = s_urlUploadRegex.Match(text.Replace(@"\/", "/"));
            if (uploadUrlMatch.Success)
            {
                UploadUrl = uploadUrlMatch.Groups[1].Value;
            }
            else
            {
                return false;
            }

            var fileIdMatch = s_fileIdRegex.Match(text);
            if (fileIdMatch.Success)
            {
                FileId = fileIdMatch.Groups[1].Value;
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}
