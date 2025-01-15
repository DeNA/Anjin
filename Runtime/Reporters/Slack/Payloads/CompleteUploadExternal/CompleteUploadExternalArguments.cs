// Copyright (c) 2023-2025 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace DeNA.Anjin.Reporters.Slack.Payloads.CompleteUploadExternal
{
    /// <summary>
    /// Arguments of files.completeUploadExternal API.
    /// </summary>
    /// <see href="https://api.slack.com/methods/files.completeUploadExternal"/>
    [Serializable]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class CompleteUploadExternalArguments
    {
        public UploadFileInfo[] files;
        public string channel_id;
        public string initial_comment;
        public string thread_ts;

        public CompleteUploadExternalArguments(string fileId, string channelId, string threadTs)
        {
            files = new[] { new UploadFileInfo { id = fileId } };
            channel_id = channelId;
            initial_comment = null;
            thread_ts = threadTs;
        }

        public string ToJson(bool prettyPrint = false)
        {
            return JsonUtility.ToJson(this, prettyPrint);
        }
    }
}
