using System;
using System.Collections.Generic;
using System.Text;
using ahd.Graphite.Base;

namespace ahd.Graphite.Path
{
    /// <summary>
    /// base class for extended graphite targets
    /// </summary>
    public abstract class ModifiedGraphitePath : GraphitePath
    {
        /// <summary>
        /// base target
        /// </summary>
        protected readonly GraphitePath Previous;

        /// <summary>
        /// base class for extended graphite targets
        /// </summary>
        /// <param name="name">name of the extension</param>
        /// <param name="previous">base target</param>
        protected ModifiedGraphitePath(string name, GraphitePath previous) : base(name)
        {
            Previous = previous;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var sb = new StringBuilder();
            ToStringBuilder(sb);
            return sb.ToString();
        }
    }
}