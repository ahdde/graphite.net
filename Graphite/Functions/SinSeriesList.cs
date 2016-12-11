using ahd.Graphite.Base;

namespace ahd.Graphite.Functions
{
    /// <summary>
    /// Just returns the sine of the current time. The optional amplitude parameter changes the amplitude of the wave.
    /// </summary>
    public class SinSeriesList : SeriesListBase
    {
        public SinSeriesList(string name, int amplitude=1, uint step=60)
        {
            Name = name;
            Amplitude = amplitude;
            Step = step;
        }

        public string Name { get; }

        public int Amplitude { get; }

        public uint Step { get; }

        public override string ToString()
        {
            if (Step == 60)
                return $"sin(\"{Name}\",{Amplitude})";
            return $"sin(\"{Name}\",{Amplitude},{Step})";
        }
    }
}