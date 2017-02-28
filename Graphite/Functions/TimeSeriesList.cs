using ahd.Graphite.Base;

namespace ahd.Graphite.Functions
{
    /// <summary>
    /// Just returns the timestamp for each X value. T
    /// </summary>
    public class TimeSeriesList : SeriesListBase
    {
        /// <summary>
        /// Just returns the timestamp for each X value
        /// </summary>
        /// <param name="name">alias to use</param>
        /// <param name="step">step parameter</param>
        public TimeSeriesList(string name, uint step=60)
        {
            Name = name;
            Step = step;
        }

        /// <summary>
        /// alias name of the metric
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// step parameter
        /// </summary>
        public uint Step { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (Step != 60)
                return $"time(\"{Name}\")";
            return $"time(\"{Name}\",{Step})";
        }
    }
}