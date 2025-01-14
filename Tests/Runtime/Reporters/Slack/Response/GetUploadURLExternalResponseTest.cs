// Copyright (c) 2023-2025 DeNA Co., Ltd.
// This software is released under the MIT License.

using NUnit.Framework;

namespace DeNA.Anjin.Reporters.Slack.Response
{
    [TestFixture]
    public class GetUploadURLExternalResponseTest
    {
        [Test]
        public void ParseResponse_okIsFalse_NotSuccess()
        {
            const string Body = "{\"ok\":false}";

            var sut = GetUploadURLExternalResponse.FromJson(Body);
            Assert.That(sut.ok, Is.False);
        }

        [Test]
        public void ParseResponse_okIsTrue_Success()
        {
            const string Body = "{\"ok\":true}";

            var sut = GetUploadURLExternalResponse.FromJson(Body);
            Assert.That(sut.ok, Is.True);
        }

        [Test]
        public void ParseResponse_ExistUploadUrl_SetUploadUrl()
        {
            const string UploadUrl = @"https:\/\/files.slack.com\/upload\/v1\/ABC123-abc123_abc123";
            const string FileId = "F123ABC456";
            var body = "{" + $"\"ok\":true,\"upload_url\":\"{UploadUrl}\",\"file_id\":\"{FileId}\"" + "}";

            var sut = GetUploadURLExternalResponse.FromJson(body);
            Assert.That(sut.upload_url, Is.EqualTo(UploadUrl.Replace(@"\/", "/")));
        }

        [Test]
        public void ParseResponse_ExistFileId_SetFileId()
        {
            const string UploadUrl = @"https:\/\/files.slack.com\/upload\/v1\/ABC123-abc123_abc123";
            const string FileId = "F123ABC456";
            var body = "{" + $"\"ok\":true,\"upload_url\":\"{UploadUrl}\",\"file_id\":\"{FileId}\"" + "}";

            var sut = GetUploadURLExternalResponse.FromJson(body);
            Assert.That(sut.file_id, Is.EqualTo(FileId));
        }
    }
}
