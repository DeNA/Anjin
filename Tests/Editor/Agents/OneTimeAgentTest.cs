// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Collections;
using DeNA.Anjin.Agents;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.TestTools;

namespace DeNA.Anjin.Editor.Agents
{
    /// <summary>
    /// Verify reset <c>OneTimeAgent</c>'s executed flag
    /// </summary>
    public class OneTimeAgentTest
    {
        private OneTimeAgent LoadTestAgent()
        {
            return AssetDatabase.LoadAssetAtPath<OneTimeAgent>(
                "Packages/com.dena.anjin/Tests/TestAssets/OneTimeAgentForTests.asset");
        }

        [UnityTest]
        public IEnumerator ResetExecutedFlag_whenEnterPlayMode_reset()
        {
            var sut = LoadTestAgent();
            sut.wasExecuted = true;

            yield return new EnterPlayMode();

            sut = LoadTestAgent(); // Reload because domain reloaded
            Assert.That(sut.wasExecuted, Is.False); // was reset
        }
    }
}
