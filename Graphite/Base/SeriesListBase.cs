using System;
using System.Collections.Generic;
using System.Linq;
using ahd.Graphite.Functions;

namespace ahd.Graphite.Base
{
    /// <summary>
    /// graphite target
    /// </summary>
    public abstract partial class SeriesListBase
    {
        /// <inheritdoc />
        public abstract override string ToString();

        private static object[] Merge<T>(object value, params T[] values)
        {
            if (values.Length == 0) return new[] {value};
            return Merge(value, Array.ConvertAll<T, object>(values, x => x));
        }

        /// <summary>
        /// merge parameter
        /// </summary>
        protected static object[] Merge(object value, params object[] values)
        {
            if (values.Length == 0) return new[] {value};
            var parameter = new List<object> {value};
            parameter.AddRange(values);
            return parameter.ToArray();
        }
        
        /// <summary>
        /// add single quotes
        /// </summary>
        protected static string SingleQuote(string value)
        {
            if (value is null) return null;
            return $"'{value}'";
        }

        private SeriesListFunction Unary(string functionName)
        {
            return new SeriesListFunction(functionName, this);
        }

        private SeriesListFunction Binary(string functionName, object parameter)
        {
            if (parameter is null) return Unary(functionName);
            return new SeriesListFunction(functionName, this, parameter);
        }

        private SeriesListFunction Ternary(string functionName, object parameter1, object parameter2)
        {
            if (parameter2 is null) return Binary(functionName, parameter1);
            return new SeriesListFunction(functionName, this, parameter1, parameter2);
        }

        private SeriesListFunction Quaternary(string functionName, object parameter1, object parameter2, object parameter3)
        {
            if (parameter3 is null) return Ternary(functionName, parameter1, parameter2);
            return new SeriesListFunction(functionName, this, parameter1, parameter2, parameter3);
        }

        private SeriesListFunction Multiple(string functionName, params object[] values)
        {
            if (values is null || values.Length == 0) return Unary(functionName);
            if (values.Length == 1) return Binary(functionName, values[0]);
            if (values.Length == 2) return Ternary(functionName, values[0], values[1]);
            if (values.Length == 3) return Quaternary(functionName, values[0], values[1], values[2]);
            return new SeriesListFunction(functionName, Merge(this, values));
        }

        /// <summary>
        /// Takes a seriesList and applies some complicated function (described by a string), replacing templates with unique prefixes of keys from the seriesList (the key is all nodes up to the index given as nodeNum).
        /// </summary>
        /// <param name="nodeNum"></param>
        /// <param name="templateFunction"></param>
        /// <param name="newName">If the newName parameter is provided, the name of the resulting series will be given by that parameter, with any “%” characters replaced by the unique prefix.</param>
        /// <returns></returns>
        public SeriesListBase ApplyByNode(uint nodeNum, SeriesListFunction templateFunction, string newName = null)
        {
            return newName == null
                ? Ternary("applyByNode", nodeNum, SingleQuote(templateFunction.ToString()))
                : Quaternary("applyByNode", nodeNum, SingleQuote(templateFunction.ToString()), SingleQuote(newName));
        }

        /// <summary>
        /// Calculates a percentage of the total of a wildcard series. If `total` is specified,
        /// each series will be calculated as a percentage of that total. If `total` is not specified,
        /// the sum of all points in the wildcard series will be used instead.
        /// </summary>
        public SeriesListFunction AsPercent(int total)
        {
            return Binary("asPercent", total);
        }

        /// <summary>
        /// Takes a serieslist and maps a callback to subgroups within as defined by a common node
        /// </summary>
        public SeriesListFunction GroupByNode(uint node, Func<SeriesListBase, Func<SeriesListFunction>> callback)
        {
            var result = callback(new GraphitePath("unused")).Invoke();
            return Ternary("groupByNode", node, SingleQuote(result.FunctionName));
        }

        /// <summary>
        /// Takes a serieslist and maps a callback to subgroups within as defined by multiple nodes
        /// </summary>
        public SeriesListFunction GroupByNodes(Func<SeriesListBase, Func<SeriesListFunction>> callback, params int[] nodes)
        {
            var result = callback(new GraphitePath("unused")).Invoke();
            return new SeriesListFunction("groupByNodes", Merge(this, Merge(SingleQuote(result.FunctionName), nodes)));
        }

        /// <summary>
        /// Smarter version of summarize.
        /// </summary>
        public SeriesListFunction SmartSummarize(string interval, Func<SeriesListBase, Func<SeriesListFunction>> callback)
        {
            var func = callback(new GraphitePath("unused")).Invoke();
            return Ternary("smartSummarize", SingleQuote(interval), SingleQuote(func.FunctionName));
        }
        
        /// <summary>
        /// allows the metric paths to contain named variables.
        /// Variable values can be specified or overriden in <see cref="GraphiteClient.GetMetricsDataAsync(ahd.Graphite.Base.SeriesListBase,string,string,System.Collections.Generic.IDictionary{string,string},System.Nullable{ulong},System.Threading.CancellationToken)"/>
        /// </summary>
        /// <param name="defaultValues">default values for the template variables</param>
        /// <returns></returns>
        public SeriesListFunction Template(params Tuple<string, string>[] defaultValues)
        {
            return new SeriesListFunction("template", Merge(this, defaultValues.Select(x => $"{x.Item1}={SingleQuote(x.Item2)}").ToArray()));
        }

        /// <summary>
        /// allows the metric paths to contain positional variables.
        /// Variable values can be specified or overriden in <see cref="GraphiteClient.GetMetricsDataAsync(ahd.Graphite.Base.SeriesListBase,string,string,System.Collections.Generic.IDictionary{string,string},System.Nullable{ulong},System.Threading.CancellationToken)"/>
        /// </summary>
        /// <param name="defaultValues">default values for the template variables</param>
        /// <returns></returns>
        public SeriesListFunction Template(params string[] defaultValues)
        {
            return new SeriesListFunction("template", Merge(this, defaultValues.Select(SingleQuote).ToArray()));  
        }

        /// <summary>
		/// Short form: ``reduce()``
		/// <para>
		/// Takes a list of seriesLists and reduces it to a list of series by means of the reduceFunction.
		/// </para>
		/// <para>
		/// Reduction is performed by matching the reduceNode in each series against the list of
		/// reduceMatchers. Then each series is passed to the reduceFunction as arguments in the order
		/// given by reduceMatchers. The reduceFunction should yield a single series.
		/// </para>
		/// <para>
		/// The resulting list of series are aliased so that they can easily be nested in other functions.
		/// </para>
		/// <para>
		/// **Example**: Map/Reduce asPercent(bytes_used,total_bytes) for each server
		/// </para>
		/// <para>
		/// Assume that metrics in the form below exist:
		/// </para>
		/// <code>
		///      servers.server1.disk.bytes_used
		///      servers.server1.disk.total_bytes
		///      servers.server2.disk.bytes_used
		///      servers.server2.disk.total_bytes
		///      servers.server3.disk.bytes_used
		///      servers.server3.disk.total_bytes
		///      ...
		///      servers.serverN.disk.bytes_used
		///      servers.serverN.disk.total_bytes
		/// </code>
		/// <para>
		/// To get the percentage of disk used for each server:
		/// </para>
		/// <code>
		///     reduceSeries(mapSeries(servers.*.disk.*,1),"asPercent",3,"bytes_used","total_bytes") =&gt;
		/// </code>
		/// <para>
		///       alias(asPercent(servers.server1.disk.bytes_used,servers.server1.disk.total_bytes),"servers.server1.disk.reduce.asPercent"),
		///       alias(asPercent(servers.server2.disk.bytes_used,servers.server2.disk.total_bytes),"servers.server2.disk.reduce.asPercent"),
		///       alias(asPercent(servers.server3.disk.bytes_used,servers.server3.disk.total_bytes),"servers.server3.disk.reduce.asPercent"),
		///       ...
		///       alias(asPercent(servers.serverN.disk.bytes_used,servers.serverN.disk.total_bytes),"servers.serverN.disk.reduce.asPercent")
		/// </para>
		/// <para>
		/// In other words, we will get back the following metrics::
		/// </para>
		/// <para>
		///     servers.server1.disk.reduce.asPercent
		///     servers.server2.disk.reduce.asPercent
		///     servers.server3.disk.reduce.asPercent
		///     ...
		///     servers.serverN.disk.reduce.asPercent
		/// </para>
		/// <para>
		/// .. seealso:: <see cref="MapSeries(uint[])"/>
		/// </para>
		/// </summary>
        public SeriesListFunction ReduceSeries(string reduceFunction, uint reduceNode, params string[] reduceMatchers)
        {
            return new ReduceSeriesSeriesList(this, reduceFunction, reduceNode, reduceMatchers);
        }

        /// <summary>
        /// lets you invoke arbitrary named functions which are not yet implemented
        /// </summary>
        /// <param name="name">graphite function name</param>
        /// <param name="parameter">function parameters</param>
        /// <returns></returns>
        [Obsolete("please open an issue to add support for this unknown function")]
        public SeriesListFunction Function(string name, params object[] parameter)
        {
            return new SeriesListFunction(name, Merge(this, parameter));
        }
    }
}
