using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ahd.Graphite
{
    /// <summary>
    /// graphite metric tree node
    /// </summary>
    [JsonConverter(typeof(GraphiteMetricConverter))]
    public class GraphiteMetric
    {
        /// <summary>
        /// </summary>
        /// <param name="path"></param>
        /// <param name="is_leaf"></param>
        /// <param name="name"></param>
        public GraphiteMetric(string path, string is_leaf, string name)
        {
            Id = path.TrimEnd('.');
            Leaf = is_leaf != "0";
            Expandable = path.EndsWith(".");
            Text = name;
        }

        /// <summary>
        /// whether the node is a leaf node
        /// </summary>
        public bool Leaf { get; }

        /// <summary>
        /// whether the node is expandable
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

    internal class GraphiteMetricConverter : JsonConverter<GraphiteMetric>
    {
        public override GraphiteMetric Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException($"Can not deserialize {nameof(GraphiteMetric)}");

            var path = string.Empty;
            var is_leaf = string.Empty;
            var name = string.Empty;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString();
                    reader.Read();
                    switch (propertyName)
                    {
                        case nameof(path):
                            path = reader.GetString();
                            break;
                        case nameof(is_leaf):
                            is_leaf = reader.GetString();
                            break;
                        case nameof(name):
                            name = reader.GetString();
                            break;
                        default:
                            throw new JsonException($"Unexpected Property Name: {propertyName}");
                    }
                }
            }

            return new GraphiteMetric(path, is_leaf, name);
        }

        public override void Write(Utf8JsonWriter writer, GraphiteMetric value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("path", value.Expandable ? value.Id + "." : value.Id);
            writer.WriteString("is_leaf", value.Leaf ? "1" : "0");
            writer.WriteString("name", value.Text);
            writer.WriteEndObject();
        }
    }

}