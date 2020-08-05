using System;
using System.Linq;

namespace ahd.Graphite.Base
{
    /// <summary>
    /// graphite function call wrapper
    /// </summary>
    public class SeriesListFunction : SeriesListBase
    {
        /// <summary>
        /// create a function call
        /// </summary>
        /// <param name="function">name of the function</param>
        /// <param name="parameter">function arguments</param>
        protected internal SeriesListFunction(string function, params object[] parameter)
        {
            FunctionName = function;
            _parameter = parameter;
        }

        private readonly object[] _parameter;

        /// <summary>
        /// Name of the function to call
        /// </summary>
        protected internal readonly string FunctionName;

        /// <summary>
        /// format the function call
        /// </summary>
        /// <returns>the graphite string representation of the function call</returns>
        protected string FunctionCall()
        {
            var paramValues = String.Join(",", _parameter.Where(x=>!(x is null)));
            return $"{FunctionName}({paramValues})";
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return FunctionCall();
        }
    }
}