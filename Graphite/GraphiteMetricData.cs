using System;
using System.Text.Json.Serialization;

namespace ahd.Graphite
{
    /// <summary>
    /// Metric result
    /// </summary>
    public class GraphiteMetricData
    {
        /// <summary>
        /// Creates a new Metric result for the specified target
        /// </summary>
        /// <param name="target">target name</param>
        /// <param name="datapoints">timestamped values</param>
        public GraphiteMetricData(string target, MetricDatapoint[] datapoints)
        {
            Target = target;
            Datapoints = datapoints;
        }

        /// <summary>
        /// Target name, as returned from graphite
        /// </summary>
        [JsonPropertyName("target")]
        public string Target { get; }

        /// <summary>
        /// List of timestamped values as returned from graphite
        /// </summary>
        [JsonPropertyName("datapoints")]
        public MetricDatapoint[] Datapoints { get; }
    }
}