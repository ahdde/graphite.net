using System;
using Newtonsoft.Json;

namespace ahd.Graphite
{
    /// <summary>
    /// graphite metric tree node
    /// </summary>
    public class GraphiteMetric
    {
        /// <summary>
        /// </summary>
        /// <param name="path"></param>
        /// <param name="is_leaf"></param>
        /// <param name="name"></param>
        [JsonConstructor]
        public GraphiteMetric(string path, string is_leaf, string name)
        {
            Id = path.TrimEnd('.');
            Leaf = is_leaf != "0";
            Expandable = path.EndsWith(".");
            Text = name;
        }

        /// <summary>
        /// wether the node is a leaf node
        /// </summary>
        public bool Leaf { get; }

        /// <summary>
        /// wether the node is expandable
        /// </summary>
        public bool Expandable { get; }

        /// <summary>
        /// id of the current node
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// name of the current node
        /// </summary>
        public string Text { get; }
    }
}