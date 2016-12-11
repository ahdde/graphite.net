using System;
using ahd.Graphite.Base;

namespace ahd.Graphite.Path
{
    internal class WildcardGraphitePath : ModifiedGraphitePath
    {
        protected internal WildcardGraphitePath(GraphitePath previous) : base(String.Empty, previous)
        {
        }

        public override string ToString()
        {
            return Previous + "*";
        }

        public override GraphitePath Wildcard()
        {
            return this;
        }
    }
}