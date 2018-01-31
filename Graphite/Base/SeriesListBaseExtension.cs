using System.Collections.Generic;
using System.Linq;

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
        public static SeriesListFunction Sum(this SeriesListBase[] series)
        {
            return new SeriesListFunction("sum", series);
        }

        /// <summary>
        /// This will add metrics together and return the sum at each datapoint.
        /// </summary>
        public static SeriesListFunction Sum(this IEnumerable<SeriesListBase> series)
        {
            return series.ToArray().Sum();
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList. For each datapoint from each metric passed in, pick the minimum value and graph it.
        /// </summary>
        public static SeriesListFunction MinSeries(this SeriesListBase[] series)
        {
            return new SeriesListFunction("minSeries", series);
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList. For each datapoint from each metric passed in, pick the minimum value and graph it.
        /// </summary>
        public static SeriesListFunction MinSeries(this IEnumerable<SeriesListBase> series)
        {
            return series.ToArray().MinSeries();
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList. For each datapoint from each metric passed in, pick the maximum value and graph it.
        /// </summary>
        public static SeriesListFunction MaxSeries(this SeriesListBase[] series)
        {
            return new SeriesListFunction("maxSeries", series);
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList. For each datapoint from each metric passed in, pick the maximum value and graph it.
        /// </summary>
        public static SeriesListFunction MaxSeries(this IEnumerable<SeriesListBase> series)
        {
            return series.ToArray().MaxSeries();
        }

        /// <summary>
        /// Takes an arbitrary number of seriesLists and adds them to a single seriesList. This is used to pass multiple seriesLists to a function which only takes one
        /// </summary>
        public static SeriesListFunction Group(this SeriesListBase[] series)
        {
            return new SeriesListFunction("group", series);
        }

        /// <summary>
        /// Takes an arbitrary number of seriesLists and adds them to a single seriesList. This is used to pass multiple seriesLists to a function which only takes one
        /// </summary>
        public static SeriesListFunction Group(this IEnumerable<SeriesListBase> series)
        {
            return series.ToArray().Group();
        }
    }
}