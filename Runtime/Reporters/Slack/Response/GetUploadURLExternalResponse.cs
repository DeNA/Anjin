// Copyright (c) 2023-2025 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using UnityEngine;
using UnityEngine.Networking;

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

        /// <summary>
        /// Create instance from UnityWebRequest.
        /// </summary>
        /// <param name="www">UnityWebRequest with response</param>
        /// <returns></returns>
        public static GetUploadURLExternalResponse FromWebResponse(UnityWebRequest www)
        {
#if UNITY_2020_2_OR_NEWER
            if (www.result != UnityWebRequest.Result.Success)
            {
                return new GetUploadURLExternalResponse() { ok = false };
            }
#else
            if (www.isNetworkError || www.isHttpError)
            {
                return new GetUploadURLExternalResponse() { ok = false };
            }
#endif
            return FromJson(www.downloadHandler.text);
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
