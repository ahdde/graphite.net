using ahd.Graphite.Base;

namespace ahd.Graphite.Functions
{
    /// <summary>
    /// Just returns the timestamp for each X value. T
    /// </summary>
    public class TimeSeriesList : SeriesListBase
    {
        public TimeSeriesList(string name, uint step=60)
        {
            Name = name;
            Step = step;
        }

        public string Name { get; }

        public uint Step { get; }

        public override string ToString()
        {
            if (Step != 60)
                return $"time(\"{Name}\")";
            return $"time(\"{Name}\",{Step})";
        }
    }
}