// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using JetBrains.Annotations;
using UnityEngine;

namespace DeNA.Anjin.Settings
{
    /// <summary>
    /// Scene picker attribute.
    /// 
    /// This attribute is attach to the `string` field and set the path to the Scene file.
    /// </summary>
    public sealed class ScenePickerAttribute : PropertyAttribute
    {
        /// <summary>
        /// Display label.
        /// 
        /// Set If a label different from the field name is specified.
        /// </summary>
        [CanBeNull] public readonly string Label;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="label">Set If a label different from the field name is specified</param>
        public ScenePickerAttribute(string label = null)
        {
            Label = label;
        }
    }
}
