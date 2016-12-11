using ahd.Graphite.Base;

namespace ahd.Graphite.Functions
{
    /// <summary>
    /// Returns a random walk starting at 0. This is great for testing when there is no real data in whisper.
    /// </summary>
    public class RandomWalkSeriesList : SeriesListBase
    {
        public RandomWalkSeriesList(string name, uint step=60)
        {
            Name = name;
            Step = step;
        }

        public string Name { get; }

        public uint Step { get; }

        public override string ToString()
        {
            if (Step == 60)
                return $"randomWalk(\"{Name}\")";
            return $"randomWalk(\"{Name}\", {Step})";
        }
    }
}