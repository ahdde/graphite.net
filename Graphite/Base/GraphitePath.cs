using System;
using ahd.Graphite.Path;

namespace ahd.Graphite.Base
{
    public class GraphitePath : SeriesListBase
    {
        private static readonly char[] ReservedChars = { '[', ']', '*', '.' };

        public GraphitePath(string name):this(name, true)
        {
        }

        private GraphitePath(string name, bool checkPath)
        {
            if (checkPath && name.IndexOfAny(ReservedChars) >= 0)
                throw new InvalidOperationException();
            Name = name;
        }

        public static GraphitePath Parse(string path)
        {
            return new GraphitePath(path, false);
        }

        public string Name { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name ?? String.Empty;
        }

        public GraphitePath Path(string name)
        {
            return new PathGraphitePath(name, this);
        }

        public virtual GraphitePath Wildcard()
        {
            return new WildcardGraphitePath(this);
        }

        public GraphitePath WildcardPath()
        {
            return Path(String.Empty).Wildcard();
        }

        public virtual GraphitePath Range(char start, char end)
        {
            return new RangeGraphitePath(new string(new[] { start, '-', end }), this);
        }

        public GraphitePath RangePath(char start, char end)
        {
            return Path(String.Empty).Range(start, end);
        }

        public virtual GraphitePath Chars(params char[] chars)
        {
            return new CharsGraphitePath(new string(chars), this);
        }

        public GraphitePath CharsPath(params char[] chars)
        {
            return Path(String.Empty).Chars(chars);
        }

        public virtual GraphitePath Value(params string[] value)
        {
            return new ValueGraphitePath(String.Join(",", value), this);
        }

        public GraphitePath ValuePath(params string[] value)
        {
            return Path(String.Empty).Value(value);
        }
    }
}