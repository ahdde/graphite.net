namespace ahd.Graphite.Base
{
    /// <summary>
    /// Helper extension methods
    /// </summary>
    public static class SeriesListBaseExtension
    {
        /// <summary>
        /// This will add metrics together and return the sum at each datapoint.
        /// </summary>
        /// <returns></returns>
        public static SeriesListFunction Sum(this SeriesListBase[] series)
        {
            return new SeriesListFunction("sum", series);
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList. For each datapoint from each metric passed in, pick the minimum value and graph it.
        /// </summary>
        public static SeriesListFunction MinSeries(this SeriesListBase[] series)
        {
            return new SeriesListFunction("minSeries", series);
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList. For each datapoint from each metric passed in, pick the maximum value and graph it.
        /// </summary>
        public static SeriesListFunction MaxSeries(this SeriesListBase[] series)
        {
            return new SeriesListFunction("maxSeries", series);
        }
    }
}