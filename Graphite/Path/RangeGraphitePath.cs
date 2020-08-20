using System.Text;
using ahd.Graphite.Base;

namespace ahd.Graphite.Path
{
    internal class RangeGraphitePath : ModifiedGraphitePath
    {
        protected internal RangeGraphitePath(string name, GraphitePath previous) : base(name, previous)
        {
        }

        internal override void ToStringBuilder(StringBuilder builder)
        {
            Previous.ToStringBuilder(builder);
            builder.Append('[');
            builder.Append(Name);
            builder.Append(']');
        }

        public override GraphitePath Range(char start, char end)
        {
            var builder = new StringBuilder(Name);
            builder.Append(start).Append('-').Append(end);
            return new RangeGraphitePath(builder.ToString(), Previous);
        }
    }
}