using System;
using System.Collections.Generic;
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
