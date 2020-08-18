using System.Text;
using ahd.Graphite.Base;

namespace ahd.Graphite.Path
{
    internal class PathGraphitePath : ModifiedGraphitePath
    {
        protected internal PathGraphitePath(string name, GraphitePath previous) : base(name, previous)
        {
        }

        internal override void ToStringBuilder(StringBuilder builder)
        {
            Previous.ToStringBuilder(builder);
            builder.Append('.');
            builder.Append(Name);
        }
    }
}