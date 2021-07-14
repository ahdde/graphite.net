using System.Text.Json.Serialization;

namespace ahd.Graphite
{
    internal class GraphiteExpandResult
    {
        [JsonPropertyName("results")]
        public string[] Results { get; set; }
    }

    internal class GraphiteFindResult
    {
        [JsonPropertyName("metrics")]
        public GraphiteMetric[] Metrics { get; set; }
    }
}