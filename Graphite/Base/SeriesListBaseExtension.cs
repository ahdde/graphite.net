namespace ahd.Graphite.Base
{
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
    }
}