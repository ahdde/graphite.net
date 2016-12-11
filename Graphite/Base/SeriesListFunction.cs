using System;

namespace ahd.Graphite.Base
{
    public class SeriesListFunction : SeriesListBase
    {
        protected internal SeriesListFunction(string function, params object[] parameter)
        {
            FunctionName = function;
            _parameter = parameter;
        }

        private readonly object[] _parameter;

        protected internal readonly string FunctionName;

        protected string FunctionCall()
        {
            var paramValues = String.Join(",", _parameter);
            return $"{FunctionName}({paramValues})";
        }

        public override string ToString()
        {
            return FunctionCall();
        }
    }
}