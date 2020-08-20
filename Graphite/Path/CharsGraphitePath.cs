using System.Text;
using ahd.Graphite.Base;

namespace ahd.Graphite.Path
{
    internal class CharsGraphitePath : ModifiedGraphitePath
    {
        protected internal CharsGraphitePath(string name, GraphitePath previous) : base(name, previous)
        {
        }

        internal override void ToStringBuilder(StringBuilder builder)
        {
            Previous.ToStringBuilder(builder);
            builder.Append('[');
            builder.Append(Name);
            builder.Append(']');
        }

        public override GraphitePath Chars(params char[] chars)
        {
            var builder = new StringBuilder(Name);
            builder.Append(chars);
            return new CharsGraphitePath(builder.ToString(), Previous);
        }
    }
}