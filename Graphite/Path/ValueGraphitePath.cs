using System;
using System.Linq;
using System.Text;
using ahd.Graphite.Base;

namespace ahd.Graphite.Path
{
    internal class ValueGraphitePath : ModifiedGraphitePath
    {
        protected internal ValueGraphitePath(string name, GraphitePath previous) : base(name, previous)
        {
        }

        internal override void ToStringBuilder(StringBuilder builder)
        {
            Previous.ToStringBuilder(builder);
            builder.Append('{');
            builder.Append(Name);
            builder.Append('}');
        }

        public override GraphitePath Values(params string[] value)
        {
            return new ValueGraphitePath(String.Join(",", new[] { Name }.Concat(value)), Previous);
        }
    }
}