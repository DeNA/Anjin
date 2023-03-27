// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System;
using System.Diagnostics.CodeAnalysis;

namespace DeNA.Anjin.Editor.Definitions
{
    /// <summary>
    /// Assembly definition file (.asmdef)
    /// <see href="https://docs.unity3d.com/Manual/AssemblyDefinitionFileFormat.html"/>
    /// </summary>
    [Serializable]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "NotAccessedField.Global")]
    [SuppressMessage("Usage", "CC0034:Redundant field assignment")]
    [SuppressMessage("ReSharper", "MissingXmlDoc")]
    public class AssemblyDefinition
    {
        public bool allowUnsafeCode = false; //Optional. Defaults to false.
        public bool autoReferenced = true; //Optional. Defaults to true.
        public string[] defineConstraints; // Optional. The symbols that serve as constraints. Can be empty.
        public string[] excludePlatforms; // Optional. The platform name strings to exclude or an empty array.
        public string[] includePlatforms; // Optional. The platform name strings to exclude or an empty array.
        public string name; // Required.
        public bool noEngineReferences = false; // Optional. Defaults to false.

        public string[] optionalUnityReferences;
        // Optional. In earlier versions of Unity, this field serialized the Unity References : Test Assemblies option used to designate the assembly as a test assembly

        public bool overrideReferences = false;
        // Optional. Set to true if precompiledReferences contains values. Defaults to false.

        public string[] precompiledReferences;
        // Optional. The file names of referenced DLL libraries including extension, but without other path elements. Can be empty.
        // This array is ignored unless you set overrideReferences to true.

        public string[] references; // Optional. References to other assemblies created with Assembly Definition assets.
        public object[] versionDefines; // Optional. Contains an object for each version define.
    }
}
