// Copyright (c) 2023-2025 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DeNA.Anjin.Reporters.Slack.Response
{
    /// <summary>
    /// Response of files.getUploadURLExternal.
    /// </summary>
    /// <see href="https://api.slack.com/methods/files.getUploadURLExternal"/>
    [Serializable]
    public struct GetUploadURLExternalResponse
    {
        public bool ok;
        public string error;
        public string warning;

        // ReSharper disable InconsistentNaming
        public string upload_url;
        public string file_id;
        // ReSharper restore InconsistentNaming

        private static readonly Regex s_successRegex = new Regex("\"ok\":true");
        private static readonly Regex s_urlUploadRegex = new Regex("\"upload_url\":\"(https?://[\\w\\./-]+)\"");
        private static readonly Regex s_fileIdRegex = new Regex("\"file_id\":\"(\\w+)\"");

        /// <summary>
        /// Constructor.
        /// </summary>
        [Obsolete]
        public GetUploadURLExternalResponse(bool success = false)
        {
            ok = success;
            error = null;
            warning = null;
            upload_url = null;
            file_id = null;
        }

        /// <summary>
        /// Parse REST API response body and set properties.
        /// </summary>
        /// <param name="text">REST API response body</param>
        [Obsolete]
        public bool ParseResponse(string text)
        {
            ok = s_successRegex.Match(text).Success;
            if (!ok)
            {
                return false;
            }

            var uploadUrlMatch = s_urlUploadRegex.Match(text.Replace(@"\/", "/"));
            if (uploadUrlMatch.Success)
            {
                upload_url = uploadUrlMatch.Groups[1].Value;
            }
            else
            {
                return false;
            }

            var fileIdMatch = s_fileIdRegex.Match(text);
            if (fileIdMatch.Success)
            {
                file_id = fileIdMatch.Groups[1].Value;
            }
            else
            {
                return false;
            }

            return true;
        }

        public static GetUploadURLExternalResponse FromJson(string json)
        {
            return JsonUtility.FromJson<GetUploadURLExternalResponse>(json);
        }

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }
    }
}
