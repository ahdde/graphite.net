using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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
        /// Calculates a percentage of the total of a wildcard series. If total is specified, each series will be calculated as a percentage of that total. If total is not specified, the sum of all points in the wildcard series will be used instead.<para/>
        /// The total parameter may be a single series or a numeric value.
        /// </summary>
        public SeriesListFunction AsPercent(int total)
        {
            return Binary("asPercent", total);
        }
        
        /// <summary>
        /// Takes one metric or a wildcard seriesList followed by an integer N. Out of all metrics passed, draws only the metrics with an average value above N for the time period specified.
        /// </summary>
        public SeriesListFunction AverageAbove(double minimum)
        {
            return Binary("averageAbove", minimum.ToString("r", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList followed by an integer N. Out of all metrics passed, draws only the metrics with an average value below N for the time period specified.
        /// </summary>
        public SeriesListFunction AverageBelow(double maximum)
        {
            return Binary("averageBelow", maximum.ToString("r", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Takes a serieslist and maps a callback to subgroups within as defined by a common node
        /// </summary>
        public SeriesListFunction GroupByNode(int node, Func<SeriesListBase, SeriesListBase[], SeriesListFunction> callback)
        {
            var result = callback(new GraphitePath("unused"), null);
            return Ternary("groupByNode", node, SingleQuote(result.FunctionName));
        }

        /// <summary>
        /// Takes a serieslist and maps a callback to subgroups within as defined by multiple nodes
        /// </summary>
        public SeriesListFunction GroupByNodes(Func<SeriesListBase, SeriesListBase[], SeriesListFunction> callback, params int[] nodes)
        {
            var result = callback(new GraphitePath("unused"), null);
            return new SeriesListFunction("groupByNodes", Merge(this, Merge(SingleQuote(result.FunctionName), nodes)));
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList followed by a constant n. Draws only the metrics with a maximum value above n.
        /// </summary>
        public SeriesListFunction MaximumAbove(double value)
        {
            return Binary("maximumAbove", value.ToString("r", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList followed by a constant n. Draws only the metrics with a maximum value below n.
        /// </summary>
        public SeriesListFunction MaximumBelow(double value)
        {
            return Binary("maximumBelow", value.ToString("r", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList followed by a constant n. Draws only the metrics with a minimum value above n.
        /// </summary>
        public SeriesListFunction MinimumAbove(double value)
        {
            return Binary("minimumAbove", value.ToString("r", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList followed by a constant n. Draws only the metrics with a minimum value below n.
        /// </summary>
        public SeriesListFunction MinimumBelow(double value)
        {
            return Binary("minimumBelow", value.ToString("r", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Removes data above the given threshold from the series or list of series provided. Values above this threshold are assigned a value of None.
        /// </summary>
        public SeriesListFunction RemoveAboveValue(double percentile)
        {
            return Binary("removeAboveValue", percentile.ToString("r", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Removes data below the given threshold from the series or list of series provided. Values below this threshold are assigned a value of None.
        /// </summary>
        public SeriesListFunction RemoveBelowValue(double percentile)
        {
            return Binary("removeBelowValue", percentile.ToString("r", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList. Out of all metrics passed, draws only the metrics with not empty data
        /// </summary>
        public SeriesListFunction RemoveEmptySeries()
        {
            return Unary("removeEmptySeries");
        }

        /// <summary>
        /// Smarter version of summarize.
        /// </summary>
        public SeriesListFunction SmartSummarize(string interval, Func<SeriesListBase, SeriesListBase[], SeriesListFunction> callback)
        {
            var func = callback(new GraphitePath("unused"), null);
            return Ternary("smartSummarize", SingleQuote(interval), SingleQuote(func.FunctionName));
        }

        /// <summary>
        /// Compares the maximum of each series against the given value. If the series maximum is greater than <paramref name="value"/>, the regular expression search and replace is applied against the series name to plot a related metric<para/>
        /// e. g. given useSeriesAbove(ganglia.metric1.reqs,10,’reqs’,’time’), the response time metric will be plotted only when the maximum value of the corresponding request/s metric is > 10
        /// </summary>
        /// <param name="value"></param>
        /// <param name="search"></param>
        /// <param name="replace"></param>
        /// <returns></returns>
        public SeriesListFunction UseSeriesAbove(double value, string search, string replace)
        {
            return new SeriesListFunction("useSeriesAbove", this, value.ToString("r", CultureInfo.InvariantCulture), SingleQuote(search), SingleQuote(replace));
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
