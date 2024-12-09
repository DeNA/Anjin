// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DeNA.Anjin.TestDoubles;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DeNA.Anjin.Agents
{
    [TestFixture]
    public class TerminateAgentTest
    {
        [Test]
        public void ExitCode_NotCustom_ReturnsExitCode()
        {
            var sut = ScriptableObject.CreateInstance<TerminateAgent>();
            sut.exitCode = ExitCodeByTerminateAgent.AutopilotFailed;
            sut.customExitCode = "100"; // dummy

            Assert.That(sut.ExitCode, Is.EqualTo(ExitCode.AutopilotFailed));
        }

        [Test]
        public void ExitCode_Custom_ReturnsCustomExitCode()
        {
            var sut = ScriptableObject.CreateInstance<TerminateAgent>();
            sut.exitCode = ExitCodeByTerminateAgent.Custom;
            sut.customExitCode = "100";

            Assert.That(sut.ExitCode, Is.EqualTo((ExitCode)100));
        }

        [Test]
        public void ExitCode_CustomButNotValid_ReturnsExitCodeByTerminateAgentCustom()
        {
            var sut = ScriptableObject.CreateInstance<TerminateAgent>();
            sut.exitCode = ExitCodeByTerminateAgent.Custom;
            sut.customExitCode = "not valid";

            Assert.That(sut.ExitCode, Is.EqualTo((ExitCode)ExitCodeByTerminateAgent.Custom));
        }

        [Test]
        public async Task Run_CallTerminateAsync()
        {
            var agent = ScriptableObject.CreateInstance<TerminateAgent>();
            agent.Logger = Debug.unityLogger;
            agent.name = TestContext.CurrentContext.Test.Name;
            agent.exitCode = ExitCodeByTerminateAgent.Custom;
            agent.customExitCode = "100";
            agent.exitMessage = "Terminated!";

            var spyTerminatable = new SpyTerminatable();
            agent.AutopilotInstance = spyTerminatable;

            using (var cts = new CancellationTokenSource())
            {
                await agent.Run(cts.Token);
                await UniTask.NextFrame();
            }

            Assert.That(spyTerminatable.IsCalled, Is.True);
            Assert.That(spyTerminatable.CapturedExitCode, Is.EqualTo((ExitCode)100));
            Assert.That(spyTerminatable.CapturedMessage, Is.EqualTo("Terminated!"));
            Assert.That(spyTerminatable.CapturedReporting, Is.True);

            LogAssert.Expect(LogType.Log, $"Enter {agent.name}.Run()");
            LogAssert.Expect(LogType.Log, $"Exit {agent.name}.Run()");
        }
    }
}
