// Copyright (c) 2023-2025 DeNA Co., Ltd.
// This software is released under the MIT License.

using NUnit.Framework;

namespace DeNA.Anjin.Reporters.Slack
{
    [TestFixture]
    public class GetUploadURLExternalResponseTest
    {
        [Test]
        public void ParseResponse_okIsFalse_NotSuccess()
        {
            const string Body = "\"ok\":false";

            var sut = new GetUploadURLExternalResponse();
            sut.ParseResponse(Body);
            Assert.That(sut.Success, Is.False);
        }

        [Test]
        public void ParseResponse_okIsTrue_Success()
        {
            const string Body = "\"ok\":true";

            var sut = new GetUploadURLExternalResponse();
            sut.ParseResponse(Body);
            Assert.That(sut.Success, Is.True);
        }

        [Test]
        public void ParseResponse_ExistUploadUrl_SetUploadUrl()
        {
            const string UploadUrl = @"https:\/\/files.slack.com\/upload\/v1\/ABC123-abc123_abc123";
            const string FileId = "F123ABC456";
            var body = $"\"ok\":true,\"upload_url\":\"{UploadUrl}\",\"file_id\":\"{FileId}\"";

            var sut = new GetUploadURLExternalResponse();
            sut.ParseResponse(body);
            Assert.That(sut.UploadUrl, Is.EqualTo(UploadUrl.Replace(@"\/", "/")));
        }

        [Test]
        public void ParseResponse_ExistFileId_SetFileId()
        {
            const string UploadUrl = @"https:\/\/files.slack.com\/upload\/v1\/ABC123-abc123_abc123";
            const string FileId = "F123ABC456";
            var body = $"\"ok\":true,\"upload_url\":\"{UploadUrl}\",\"file_id\":\"{FileId}\"";

            var sut = new GetUploadURLExternalResponse();
            sut.ParseResponse(body);
            Assert.That(sut.FileId, Is.EqualTo(FileId));
        }
    }
}
