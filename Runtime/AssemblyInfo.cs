// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

#if UNITY_EDITOR
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DeNA.Anjin.Editor")]
[assembly: InternalsVisibleTo("DeNA.Anjin.Editor.Tests")]
[assembly: InternalsVisibleTo("DeNA.Anjin.Tests")]
#endif
