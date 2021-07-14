using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ahd.Graphite
{
    /// <summary>
    /// Metric result
    /// </summary>
    [JsonConverter(typeof(GraphiteMetricConverter))]
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
        public string Target { get; }

        /// <summary>
        /// List of timestamped values as returned from graphite
        /// </summary>
        public MetricDatapoint[] Datapoints { get; }

        internal class GraphiteMetricConverter : JsonConverter<GraphiteMetricData>
        {
            public override GraphiteMetricData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException($"Can not deserialize {nameof(GraphiteMetricData)}");
                var target = string.Empty;
                var datapoints = Array.Empty<MetricDatapoint>();
                while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                {
                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        var propertyName = reader.GetString();
                        reader.Read();
                       
                        switch (propertyName)
                        {
                            case nameof(target):
                                target = reader.GetString(); 
                                break;
                            case nameof(datapoints):
                                datapoints = JsonSerializer.Deserialize<MetricDatapoint[]>(ref reader);
                                break;
                            default:
                                throw new JsonException($"Unexpected Property Name: {propertyName}");
                        }
                    }
                }
                return new GraphiteMetricData(target,datapoints);
            }

            public override void Write(Utf8JsonWriter writer, GraphiteMetricData value, JsonSerializerOptions options)
            {
               writer.WriteStartObject();
               writer.WriteString("target", value.Target);
               writer.WriteStartArray("datapoints");
               foreach (var datapoint in value.Datapoints)
               {
                   JsonSerializer.Serialize(writer, datapoint, options);
               }
               writer.WriteEndArray();
               writer.WriteEndObject();
            }
        }
    }
}