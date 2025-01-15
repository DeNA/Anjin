// Copyright (c) 2023-2025 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;

namespace DeNA.Anjin.Reporters.Slack.Payloads.CompleteUploadExternal
{
    [Serializable]
    public class UploadFileInfo
    {
        public string id;
        public string title;
    }
}
