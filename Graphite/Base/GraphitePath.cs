using System;
using System.Text;
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
        /// to create a nested target or wildcard target use the corresponding functions <see cref="Range"/>, <see cref="Wildcard"/>, <see cref="Chars"/> and <see cref="Values"/>
        /// </summary>
        /// <param name="name">name of the target- cannot contain '[', ']', '*', '.' (<see cref="ReservedChars"/>)/></param>
        public GraphitePath(string name):this(name, true)
        {
        }

        private GraphitePath(string name, bool checkPath)
        {
            if (checkPath && name.IndexOfAny(ReservedChars) >= 0)
                throw new InvalidOperationException();
            Name = name;
        }

        internal virtual void ToStringBuilder(StringBuilder builder)
        {
            builder.Append(Name ?? String.Empty);
        }

        /// <summary>
        /// accepts a complete target (including wildcards and functions) and wraps it in a <see cref="GraphitePath"/>
        /// </summary>
        /// <param name="path">target name - the target is neither parsed nor validated</param>
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
        [Obsolete("use Dot(...)")]
        public GraphitePath Path(string name)
        {
            return Dot(name);
        }

        /// <summary>
        /// appends a part to the current target
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public GraphitePath Dot(string name)
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
        [Obsolete("use DotWildcard(...)")]
        public GraphitePath WildcardPath()
        {
            return DotWildcard();
        }

        /// <summary>
        /// appends a wildcardpath to the current target
        /// </summary>
        /// <returns></returns>
        public GraphitePath DotWildcard()
        {
            return Dot(String.Empty).Wildcard();
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
        [Obsolete("use DotRange(...)")]
        public GraphitePath RangePath(char start, char end)
        {
            return DotRange(start, end);
        }

        /// <summary>
        /// appends a range path to the current target
        /// </summary>
        /// <param name="start">first char of the range</param>
        /// <param name="end">last char of the range</param>
        /// <returns></returns>
        public GraphitePath DotRange(char start, char end)
        {
            return Dot(String.Empty).Range(start, end);
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
        [Obsolete("use DotChars(...)")]
        public GraphitePath CharsPath(params char[] chars)
        {
            return DotChars(chars);
        }

        /// <summary>
        /// returns a char list path to the current target
        /// </summary>
        /// <param name="chars">list of chars</param>
        /// <returns></returns>
        public GraphitePath DotChars(params char[] chars)
        {
            return Dot(String.Empty).Chars(chars);
        }

        /// <summary>
        /// appends a value list to the current target
        /// </summary>
        /// <param name="value">list of values</param>
        /// <returns></returns>
        [Obsolete("use Values(...)")]
        public GraphitePath Value(params string[] value)
        {
            return Values(value);
        }

        /// <summary>
        /// appends a value list to the current target
        /// </summary>
        /// <param name="values">list of values</param>
        /// <returns></returns>
        public virtual GraphitePath Values(params string[] values)
        {
            return new ValueGraphitePath(String.Join(",", values), this);
        }

        /// <summary>
        /// appends a value list path to the current target
        /// </summary>
        /// <param name="value">list of values</param>
        /// <returns></returns>
        [Obsolete("use DotValues(...)")]
        public GraphitePath ValuePath(params string[] value)
        {
            return DotValues(value);
        }

        /// <summary>
        /// appends a value list path to the current target
        /// </summary>
        /// <param name="values">list of values</param>
        /// <returns></returns>
        public GraphitePath DotValues(params string[] values)
        {
            return Dot(String.Empty).Values(values);
        }
    }
}