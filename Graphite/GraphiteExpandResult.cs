namespace ahd.Graphite
{
    internal class GraphiteExpandResult
    {
        public string[] Results { get; set; }
    }

    internal class GraphiteFindResult
    {
        public GraphiteMetric[] Metrics { get; set; }
    }
}