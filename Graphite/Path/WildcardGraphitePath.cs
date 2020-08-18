using System;
using System.Text;
using ahd.Graphite.Base;

namespace ahd.Graphite.Path
{
    internal class WildcardGraphitePath : ModifiedGraphitePath
    {
        protected internal WildcardGraphitePath(GraphitePath previous) : base(String.Empty, previous)
        {
        }

        internal override void ToStringBuilder(StringBuilder builder)
        {
            Previous.ToStringBuilder(builder);
            builder.Append('*');
        }

        public override GraphitePath Wildcard()
        {
            return this;
        }
    }
}