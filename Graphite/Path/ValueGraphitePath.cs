using System;
using System.Linq;
using ahd.Graphite.Base;

namespace ahd.Graphite.Path
{
    internal class ValueGraphitePath : ModifiedGraphitePath
    {
        protected internal ValueGraphitePath(string name, GraphitePath previous) : base(name, previous)
        {
        }

        public override string ToString()
        {
            return $"{Previous}{{{Name}}}";
        }

        public override GraphitePath Values(params string[] value)
        {
            return new ValueGraphitePath(String.Join(",", new[] { Name }.Concat(value)), Previous);
        }
    }
}