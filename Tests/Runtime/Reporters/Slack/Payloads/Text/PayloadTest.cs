// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using NUnit.Framework;
using UnityEngine;

namespace DeNA.Anjin.Reporters.Slack.Payloads.Text
{
    [TestFixture]
    public class PayloadTest
    {
        [Test]
        public void ToJson_PlainText()
        {
            var payload = Payload.CreatePayload(
                "#channel",
                "lead",
                new Attachment[]
                {
                    Attachment.CreateAttachment(
                        new ContextBlock[]
                        {
                            ContextBlock.CreateContextBlock(
                                new Text[] { Text.CreatePlainText("message") }
                            )
                        },
                        Color.magenta
                    )
                },
                "ts"
            );
            var json = payload.ToJson(true);
            Debug.Log(json);

            Assert.That(json, Is.EqualTo(@"{
    ""channel"": ""#channel"",
    ""text"": ""lead"",
    ""attachments"": [
        {
            ""blocks"": [
                {
                    ""type"": ""context"",
                    ""elements"": [
                        {
                            ""type"": ""plain_text"",
                            ""text"": ""message""
                        }
                    ]
                }
            ],
            ""color"": ""#ff00ff""
        }
    ],
    ""thread_ts"": ""ts""
}"));
        }

        [Test]
        public void ToJson_PlainTextWithoutText()
        {
            var payload = Payload.CreatePayload(
                "#channel",
                null, // focus of this test
                null, // it becomes noise
                "ts"
            );
            var json = payload.ToJson(true);
            Debug.Log(json);

            Assert.That(json, Is.EqualTo(@"{
    ""channel"": ""#channel"",
    ""text"": """",
    ""attachments"": [],
    ""thread_ts"": ""ts""
}"));
        }
    }
}
