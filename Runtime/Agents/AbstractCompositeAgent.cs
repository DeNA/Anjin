// Copyright (c) 2023 DeNA Co., Ltd.
// This software is released under the MIT License.

using System.Collections.Generic;
using DeNA.Anjin.Utilities;

namespace DeNA.Anjin.Agents
{
    /// <summary>
    /// Abstract Composite Agent.
    /// Implements: <see cref="ParallelCompositeAgent"/> and <see cref="SerialCompositeAgent"/>.
    /// </summary>
    public abstract class AbstractCompositeAgent : AbstractAgent
    {
        /// <summary>
        /// Agents in composite
        /// </summary>
        public List<AbstractAgent> agents;

        /// <summary>
        /// Random factory
        /// </summary>
        protected RandomFactory RandomFactory { get; private set; }

        /// <inheritdoc />
        public override IRandom Random
        {
            internal set
            {
                base.Random = value;
                RandomFactory = new RandomFactory(base.Random.Next());
            }
        }
    }
}
