using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ahd.Graphite
{
    [JsonArray]
    public class MetricDatapoint
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        
        public double? Value { get; }

        public DateTime Timestamp
        {
            get { return Epoch.AddSeconds(UnixTimestamp); }
        }

        public long UnixTimestamp { get; }
        
        [JsonConstructor]
        public MetricDatapoint(IList<object> json)
        {
            Value = (double?)json[0];
            UnixTimestamp = (long) json[1];
        }

        public MetricDatapoint(double value, long timestamp)
        {
            Value = value;
            UnixTimestamp = timestamp;
        }
    }
}