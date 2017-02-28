using ahd.Graphite.Base;

namespace ahd.Graphite.Functions
{
    /// <summary>
    /// Identity function: Returns datapoints where the value equals the timestamp of the datapoint.<para/>
    /// Useful when you have another series where the value is a timestamp, and you want to compare it to the time of the datapoint, to render an age
    /// </summary>
    public class IdentitySeriesList : SeriesListBase
    {
        /// <summary>
        /// Creates an identity series where the value equals the timestamp
        /// </summary>
        /// <param name="name">alias to use</param>
        public IdentitySeriesList(string name)
        {
            Name = name;
        }

        /// <summary>
        /// alias name of the series
        /// </summary>
        public string Name { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"identity(\"{Name}\")";
        }
    }
}