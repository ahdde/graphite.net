using ahd.Graphite.Base;

namespace ahd.Graphite.Path
{
    public abstract class ModifiedGraphitePath : GraphitePath
    {
        protected readonly GraphitePath Previous;

        protected ModifiedGraphitePath(string name, GraphitePath previous) : base(name)
        {
            Previous = previous;
        }
    }
}