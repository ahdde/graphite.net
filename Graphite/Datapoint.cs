using System;

namespace ahd.Graphite
{
    public struct Datapoint : IEquatable<Datapoint>
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public readonly string Series;

        public double Value { get; }

        public DateTime Timestamp { get; }

        public long UnixTimestamp
        {
            get { return ToUnixTimestamp(Timestamp); }
        }

        public Datapoint(string series, double value, DateTime timestamp)
        {
            Series = series;
            Value = value;
            Timestamp = timestamp;
        }
        public Datapoint(string series, double value, long timestamp)
        {
            Series = series;
            Value = value;
            Timestamp = Epoch.AddSeconds(timestamp).ToLocalTime();
        }

        public bool Equals(Datapoint other)
        {
            return String.Equals(Series, other.Series) && Value.Equals(other.Value) && Timestamp.Equals(other.Timestamp);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Datapoint && Equals((Datapoint) obj);
        }

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
        public static bool operator ==(Datapoint left, Datapoint right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Datapoint left, Datapoint right)
        {
            return !left.Equals(right);
        }

        public static long ToUnixTimestamp(DateTime timestamp)
        {
            return (long)(timestamp.ToUniversalTime() - Epoch).TotalSeconds;
        }
    }
}