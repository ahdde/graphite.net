using ahd.Graphite.Base;

namespace ahd.Graphite.Functions
{
    /// <summary>
    /// Returns a random walk starting at 0. This is great for testing when there is no real data in whisper.
    /// </summary>
    public class RandomWalkSeriesList : SeriesListBase
    {
        /// <summary>
        /// Creates a random walk series starting at 0
        /// </summary>
        /// <param name="name">alias to use</param>
        /// <param name="step">step parameter</param>
        public RandomWalkSeriesList(string name, uint step=60)
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
            if (Step == 60)
                return $"randomWalk(\"{Name}\")";
            return $"randomWalk(\"{Name}\", {Step})";
        }
    }
}