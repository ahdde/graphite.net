using ahd.Graphite.Base;

namespace ahd.Graphite.Path
{
    internal class PathGraphitePath : ModifiedGraphitePath
    {
        protected internal PathGraphitePath(string name, GraphitePath previous) : base(name, previous)
        {
        }

        public override string ToString()
        {
            return $"{Previous}.{Name}";
        }
    }
}