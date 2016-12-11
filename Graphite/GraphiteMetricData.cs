using System;

namespace ahd.Graphite
{
    public class GraphiteMetricData
    {
        public GraphiteMetricData(string target, MetricDatapoint[] datapoints)
        {
            Target = target;
            Datapoints = datapoints;
        }

        public string Target { get; }

        public MetricDatapoint[] Datapoints { get; }
    }
}