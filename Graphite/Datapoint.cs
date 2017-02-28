using System;

namespace ahd.Graphite
{
    /// <summary>
    /// metric value
    /// </summary>
    public struct Datapoint : IEquatable<Datapoint>
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// target name
        /// </summary>
        public readonly string Series;

        /// <summary>
        /// value
        /// </summary>
        public double Value { get; }

        /// <summary>
        /// Timestamp for the value
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// <see cref="Timestamp"/> in seconds since unix epoch
        /// </summary>
        public long UnixTimestamp
        {
            get { return ToUnixTimestamp(Timestamp); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="series"></param>
        /// <param name="value"></param>
        /// <param name="timestamp"></param>
        public Datapoint(string series, double value, DateTime timestamp)
        {
            Series = series;
            Value = value;
            Timestamp = timestamp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="series"></param>
        /// <param name="value"></param>
        /// <param name="timestamp"></param>
        public Datapoint(string series, double value, long timestamp)
        {
            Series = series;
            Value = value;
            Timestamp = Epoch.AddSeconds(timestamp).ToLocalTime();
        }

        /// <inheritdoc/>
        public bool Equals(Datapoint other)
        {
            return String.Equals(Series, other.Series) && Value.Equals(other.Value) && Timestamp.Equals(other.Timestamp);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Datapoint && Equals((Datapoint) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Series != null ? Series.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Value.GetHashCode();
                hashCode = (hashCode*397) ^ Timestamp.GetHashCode();
                return hashCode;
            }
        }

        /// <inheritdoc />
        public static bool operator ==(Datapoint left, Datapoint right)
        {
            return left.Equals(right);
        }

        /// <inheritdoc />
        public static bool operator !=(Datapoint left, Datapoint right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Convert a <see cref="DateTime"/> to seconds since unix expoch (01/01/1970)
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static long ToUnixTimestamp(DateTime timestamp)
        {
            return (long)(timestamp.ToUniversalTime() - Epoch).TotalSeconds;
        }
    }
}