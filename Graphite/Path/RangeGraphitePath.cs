using System.Text;
using ahd.Graphite.Base;

namespace ahd.Graphite.Path
{
    internal class RangeGraphitePath : ModifiedGraphitePath
    {
        protected internal RangeGraphitePath(string name, GraphitePath previous) : base(name, previous)
        {
        }

        public override string ToString()
        {
            return $"{Previous}[{Name}]";
        }

        public override GraphitePath Range(char start, char end)
        {
            var builder = new StringBuilder(Name);
            builder.Append(start).Append('-').Append(end);
            return new RangeGraphitePath(builder.ToString(), Previous);
        }
    }
}