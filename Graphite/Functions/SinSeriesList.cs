using ahd.Graphite.Base;

namespace ahd.Graphite.Functions
{
    /// <summary>
    /// Just returns the sine of the current time. The optional amplitude parameter changes the amplitude of the wave.
    /// </summary>
    public class SinSeriesList : SeriesListBase
    {
        /// <summary>
        /// Just returns the sine of the current time.
        /// </summary>
        /// <param name="name">alias to use</param>
        /// <param name="amplitude">changes the amplitude of the wave</param>
        /// <param name="step">step parameter</param>
        public SinSeriesList(string name, int amplitude=1, uint step=60)
        {
            Name = name;
            Amplitude = amplitude;
            Step = step;
        }

        /// <summary>
        /// alias name of the metric
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// optional amplitude
        /// </summary>
        public int Amplitude { get; }

        /// <summary>
        /// step parameter
        /// </summary>
        public uint Step { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (Step == 60)
                return $"sin(\"{Name}\",{Amplitude})";
            return $"sin(\"{Name}\",{Amplitude},{Step})";
        }
    }
}