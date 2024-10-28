// Copyright (c) 2023-2024 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Collections;
using System.IO;
using DeNA.Anjin.Editor.Definitions;
using NUnit.Framework;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.TestTools;

namespace DeNA.Anjin.Editor.ContextMenu
{
    public class DoCreateGameTitleSpecificAssemblyTest
    {
        private const string AssemblyName = "TestOutputOfAnjin";
        private readonly string _path = Path.Combine("Assets", AssemblyName);

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            AssetDatabase.DisallowAutoRefresh();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            AssetDatabase.AllowAutoRefresh();
        }

        [SetUp]
        public void SetUp()
        {
            var exist = AssetDatabase.IsValidFolder(_path);
            Assume.That(exist, Is.False, "Generated folder does not exist before test");
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            var exist = AssetDatabase.IsValidFolder(_path);
            if (exist)
            {
                AssetDatabase.DeleteAsset(_path);
            }

            yield return new WaitForDomainReload();
        }

        [Test]
        public void Action_CreatedRuntimeFolderContainingAsmdef()
        {
            var sut = ScriptableObject.CreateInstance<DoCreateGameTitleSpecificAssembly>();
            sut.Action(0, _path, null);

            var asmdefPath = Path.Combine(_path, $"{AssemblyName}.asmdef");
            var asmdefFile = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(asmdefPath);
            Assert.That(asmdefFile, Is.Not.Null);

            var asmdef = new AssemblyDefinition();
            EditorJsonUtility.FromJsonOverwrite(asmdefFile.ToString(), asmdef);
            Assert.That(asmdef.name, Is.EqualTo(AssemblyName));
            Assert.That(asmdef.autoReferenced, Is.False);
            Assert.That(asmdef.defineConstraints, Does.Contain("UNITY_INCLUDE_TESTS || DENA_AUTOPILOT_ENABLE"));
            Assert.That(asmdef.includePlatforms, Is.Empty);
            Assert.That(asmdef.references, Does.Contain("DeNA.Anjin"));
            Assert.That(asmdef.references, Does.Contain("UniTask"));
        }
    }
}
