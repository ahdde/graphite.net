using System;
using ahd.Graphite.Path;

namespace ahd.Graphite.Base
{
    /// <summary>
    /// representation of the graphite target
    /// </summary>
    public class GraphitePath : SeriesListBase
    {
        private static readonly char[] ReservedChars = { '[', ']', '*', '.' };

        /// <summary>
        /// creates a new target
        /// to create a nested target or wildcard target use the corresponding functions <see cref="Range"/>, <see cref="Wildcard"/>, <see cref="Chars"/> and <see cref="Value"/>
        /// </summary>
        /// <param name="name">name of the target- cannot contain '[', ']', '*', '.'</param>
        public GraphitePath(string name):this(name, true)
        {
        }

        private GraphitePath(string name, bool checkPath)
        {
            if (checkPath && name.IndexOfAny(ReservedChars) >= 0)
                throw new InvalidOperationException();
            Name = name;
        }

        /// <summary>
        /// accepts a complete target (including wildcards and functions) and wraps it in a <see cref="GraphitePath"/>
        /// </summary>
        /// <param name="path">target name - the target is neither parsed nor vaidated</param>
        /// <returns><see cref="GraphitePath"/> wrapper for the supplied target</returns>
        public static GraphitePath Parse(string path)
        {
            return new GraphitePath(path, false);
        }

        /// <summary>
        /// Name of the target
        /// </summary>
        public string Name { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name ?? String.Empty;
        }

        /// <summary>
        /// appends a part to the current target
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public GraphitePath Path(string name)
        {
            return new PathGraphitePath(name, this);
        }

        /// <summary>
        /// appends a wildcard to the current target
        /// </summary>
        /// <returns></returns>
        public virtual GraphitePath Wildcard()
        {
            return new WildcardGraphitePath(this);
        }

        /// <summary>
        /// appends a wildcardpath to the current target
        /// </summary>
        /// <returns></returns>
        public GraphitePath WildcardPath()
        {
            return Path(String.Empty).Wildcard();
        }

        /// <summary>
        /// appends a range specifier to the current target
        /// </summary>
        /// <param name="start">first char of the range</param>
        /// <param name="end">last char of the range</param>
        /// <returns></returns>
        public virtual GraphitePath Range(char start, char end)
        {
            return new RangeGraphitePath(new string(new[] { start, '-', end }), this);
        }

        /// <summary>
        /// appends a range path to the current target
        /// </summary>
        /// <param name="start">first char of the range</param>
        /// <param name="end">last char of the range</param>
        /// <returns></returns>
        public GraphitePath RangePath(char start, char end)
        {
            return Path(String.Empty).Range(start, end);
        }

        /// <summary>
        /// appends a char list to the current target
        /// </summary>
        /// <param name="chars">list of chars</param>
        /// <returns></returns>
        public virtual GraphitePath Chars(params char[] chars)
        {
            return new CharsGraphitePath(new string(chars), this);
        }

        /// <summary>
        /// returns a char list path to the current target
        /// </summary>
        /// <param name="chars">list of chars</param>
        /// <returns></returns>
        public GraphitePath CharsPath(params char[] chars)
        {
            return Path(String.Empty).Chars(chars);
        }

        /// <summary>
        /// appends a value list to the current target
        /// </summary>
        /// <param name="value">list of values</param>
        /// <returns></returns>
        public virtual GraphitePath Value(params string[] value)
        {
            return new ValueGraphitePath(String.Join(",", value), this);
        }

        /// <summary>
        /// appends a value list path to the current target
        /// </summary>
        /// <param name="value">list of values</param>
        /// <returns></returns>
        public GraphitePath ValuePath(params string[] value)
        {
            return Path(String.Empty).Value(value);
        }
    }
}