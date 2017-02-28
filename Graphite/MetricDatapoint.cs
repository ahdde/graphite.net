using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ahd.Graphite
{
    /// <summary>
    /// time series datapoint
    /// </summary>
    [JsonArray]
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
        /// creates a datapoint from an object array as returned by graphite
        /// </summary>
        /// <param name="json">object array with two values - first value, then timestamp in unix epoch</param>
        [JsonConstructor]
        public MetricDatapoint(IList<object> json)
        {
            Value = (double?)json[0];
            UnixTimestamp = (long) json[1];
        }

        /// <summary>
        /// creates a datapoint with the specified value and timestamp
        /// </summary>
        /// <param name="value">value of the datapoint</param>
        /// <param name="timestamp">seconds since unix epoch</param>
        public MetricDatapoint(double value, long timestamp)
        {
            Value = value;
            UnixTimestamp = timestamp;
        }
    }
}