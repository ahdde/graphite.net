using System;
using Newtonsoft.Json;

namespace ahd.Graphite
{
    public class GraphiteMetric
    {
        [JsonConstructor]
        public GraphiteMetric(string path, string is_leaf, string name)
        {
            Id = path.TrimEnd('.');
            Leaf = is_leaf != "0";
            Expandable = path.EndsWith(".");
            Text = name;
        }

        public bool Leaf { get; }

        public bool Expandable { get; }

        public string Id { get; }

        public string Text { get; }
    }
}