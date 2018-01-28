using System;
using Newtonsoft.Json;

namespace ahd.Graphite
{
    /// <summary>
    /// time series datapoint
    /// </summary>
    [JsonConverter(typeof(MetricDatapointConverter))]
    public class MetricDatapoint
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        
        /// <summary>
        /// value of the datapoint
        /// </summary>
        public double? Value { get; }

        /// <summary>
        /// timestamp of the datapoint
        /// </summary>
        public DateTime Timestamp
        {
            get { return Epoch.AddSeconds(UnixTimestamp); }
        }

        /// <summary>
        /// timestamp in unix epoch (seconds since 1970)
        /// </summary>
        public long UnixTimestamp { get; }

        /// <summary>
        /// creates a datapoint with the specified value and timestamp
        /// </summary>
        /// <param name="value">value of the datapoint</param>
        /// <param name="timestamp">seconds since unix epoch</param>
        public MetricDatapoint(double? value, long timestamp)
        {
            Value = value;
            UnixTimestamp = timestamp;
        }

        public class MetricDatapointConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return typeof(MetricDatapoint).IsAssignableFrom(objectType);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.StartArray)
                {
                    if (reader.Read())
                    {
                        double? value = null;
                        switch (reader.TokenType)
                        {
                            case JsonToken.Float:
                                value = (double?)reader.Value;
                                break;
                            case JsonToken.Integer:
                                value = (double?)(long)reader.Value;
                                break;
                            case JsonToken.Null:
                                value = null;
                                break;
                            default:
                                throw new JsonSerializationException($"Unexpected Token {reader.TokenType}");
                        }
                        if (reader.Read() && reader.TokenType == JsonToken.Integer)
                        {
                            long timestamp = (long)reader.Value;
                            if (reader.Read() && reader.TokenType == JsonToken.EndArray)
                            {
                                return new MetricDatapoint(value, timestamp);
                            }
                        }
                    }
                }
                throw new JsonSerializationException("Cannot deserialize MetricDatapoint");
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                var datapoint = value as MetricDatapoint;
                if (datapoint == null)
                {
                    writer.WriteNull();
                    return;
                }
                writer.WriteStartArray();
                writer.WriteValue(datapoint.Value);
                writer.WriteValue(datapoint.UnixTimestamp);
                writer.WriteEndArray();
            }
        }
    }
}