using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ahd.Graphite.Base
{
    /// <summary>
    /// graphite target
    /// </summary>
    public abstract class SeriesListBase
    {
        /// <inheritdoc />
        public abstract override string ToString();

        private static object[] Merge<T>(object value, params T[] values)
        {
            return Merge(value, Array.ConvertAll<T, object>(values, x => x));
        }

        private static object[] Merge(object value, params object[] values)
        {
            var parameter = new List<object> {value};
            parameter.AddRange(values);
            return parameter.ToArray();
        }
        
        private static string DoubleQuote(string value)
        {
            return $"\"{value}\"";
        }
        
        private static string SingleQuote(string value)
        {
            return $"'{value}'";
        }

        private SeriesListFunction Unary(string functionName)
        {
            return new SeriesListFunction(functionName, this);
        }

        private SeriesListFunction Binary<T>(string functionName, T parameter)
        {
            return new SeriesListFunction(functionName, this, parameter);
        }

        private SeriesListFunction Ternary(string functionName, object parameter1, object parameter2)
        {
            return new SeriesListFunction(functionName, this, parameter1, parameter2);
        }

        private SeriesListFunction Quaternary(string functionName, object parameter1, object parameter2, object parameter3)
        {
            return new SeriesListFunction(functionName, this, parameter1, parameter2, parameter3);
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList and a string in quotes. Prints the string instead of the metric name in the legend.
        /// </summary>
        public SeriesListFunction Alias(string name)
        {
            return Binary("alias", DoubleQuote(name));
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList and applies the mathematical abs function to each datapoint transforming it to its absolute value.
        /// </summary>
        public SeriesListFunction Absolute()
        {
            return Unary("absolute");
        }

        /// <summary>
        /// Takes a metric or wildcard seriesList and draws a horizontal line based on the function applied to each series.<para/>
        /// <remarks>By default, the graphite renderer consolidates data points by averaging data points over time. If you are using the ‘min’ or ‘max’ function for aggregateLine, this can cause an unusual gap in the line drawn by this function and the data itself. To fix this, you should use the consolidateBy() function with the same function argument you are using for aggregateLine. This will ensure that the proper data points are retained and the graph should line up correctly.</remarks>
        /// </summary>
        public SeriesListFunction AggregateLine(string func = "avg")
        {
            return Binary("aggregateLine", SingleQuote(func));
        }

        /// <summary>
        /// Takes a seriesList and applies an alias derived from the base metric name.
        /// </summary>
        public SeriesListFunction AliasByMetric()
        {
            return Unary("aliasByMetric");
        }

        /// <summary>
        /// Takes a seriesList and applies an alias derived from one or more “node” portion/s of the target name. Node indices are 0 indexed.
        /// </summary>
        public SeriesListFunction AliasByNode(params uint[] nodes)
        {
            return new SeriesListFunction("aliasByNode", Merge(this, nodes));
        }

        /// <summary>
        /// Runs series names through a regex search/replace.
        /// </summary>
        public SeriesListFunction AliasSub(string search, string replace)
        {
            return Ternary("aliasSub", DoubleQuote(search), DoubleQuote(replace));
        }

        /// <summary>
        /// Takes a seriesList and applies some complicated function (described by a string), replacing templates with unique prefixes of keys from the seriesList (the key is all nodes up to the index given as nodeNum).
        /// </summary>
        /// <param name="nodeNum"></param>
        /// <param name="templateFunction"></param>
        /// <param name="newName">If the newName parameter is provided, the name of the resulting series will be given by that parameter, with any “%” characters replaced by the unique prefix.</param>
        /// <returns></returns>
        public SeriesListBase ApplyByNode(uint nodeNum, SeriesListFunction templateFunction, string newName = null)
        {
            return newName == null
                ? Ternary("applyByNode", nodeNum, DoubleQuote(templateFunction.ToString()))
                : Quaternary("applyByNode", nodeNum, DoubleQuote(templateFunction.ToString()), DoubleQuote(newName));
        }

        /// <summary>
        /// Calculates a percentage of the total of a wildcard series. If total is specified, each series will be calculated as a percentage of that total. If total is not specified, the sum of all points in the wildcard series will be used instead.<para/>
        /// The total parameter may be a single series or a numeric value.
        /// </summary>
        public SeriesListFunction AsPercent(int total)
        {
            return Binary("asPercent", total);
        }

        /// <summary>
        /// Calculates a percentage of the total of a wildcard series. If total is specified, each series will be calculated as a percentage of that total. If total is not specified, the sum of all points in the wildcard series will be used instead.<para/>
        /// The total parameter may be a single series or a numeric value.
        /// </summary>
        public SeriesListFunction AsPercent(SeriesListBase total)
        {
            return Binary("asPercent", total);
        }

        /// <summary>
        /// Calculates a percentage of the total of a wildcard series. If total is specified, each series will be calculated as a percentage of that total. If total is not specified, the sum of all points in the wildcard series will be used instead.<para/>
        /// The total parameter may be a single series or a numeric value.
        /// </summary>
        public SeriesListFunction AsPercent()
        {
            return Unary("asPercent");
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList followed by an integer N. Out of all metrics passed, draws only the metrics with an average value above N for the time period specified.
        /// </summary>
        public SeriesListFunction AverageAbove(double minimum)
        {
            return Binary("averageAbove", minimum.ToString("r", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList followed by an integer N. Out of all metrics passed, draws only the metrics with an average value below N for the time period specified.
        /// </summary>
        public SeriesListFunction AverageBelow(double maximum)
        {
            return Binary("averageBelow", maximum.ToString("r", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Removes functions lying inside an average percentile interval
        /// </summary>
        public SeriesListFunction AverageOutsidePercentile(int value)
        {
            return Binary("averageOutsidePercentile", value);
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList. Draws the average value of all metrics passed at each time.
        /// </summary>
        public SeriesListFunction Avg()
        {
            return Unary("avg");
        }

        /// <summary>
        /// Call <see cref="Avg"/> after inserting wildcards at the given position(s).
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public SeriesListFunction AverageSeriesWithWildcards(params int[] position)
        {
            return new SeriesListFunction("averageSeriesWithWildcards", Merge(this, position));
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList. Output 1 when the value changed, 0 when null or the same
        /// </summary>
        public SeriesListFunction Changed()
        {
            return Unary("changed");
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList and a consolidation function name.
        /// Valid function names are ‘sum’, ‘average’, ‘min’, and ‘max’.
        /// When a graph is drawn where width of the graph size in pixels is smaller than the number of datapoints to be graphed, 
        /// Graphite consolidates the values to to prevent line overlap. The consolidateBy() function changes the consolidation 
        /// function from the default of ‘average’ to one of ‘sum’, ‘max’, or ‘min’. This is especially useful in sales graphs, 
        /// where fractional values make no sense and a ‘sum’ of consolidated values is appropriate.
        /// </summary>
        public SeriesListFunction ConsolidateBy(ConsolidateFunction function = ConsolidateFunction.average)
        {
            return Binary("consolidateBy", SingleQuote(function.ToString()));
        }

        /// <summary>
        /// Draws a horizontal line representing the number of nodes found in the seriesList.
        /// </summary>
        public SeriesListFunction CountSeries()
        {
            return Unary("countSeries");
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList followed by an integer N. Out of all metrics passed, draws only the metrics whose value is above N at the end of the time period specified.
        /// </summary>
        public SeriesListFunction CurrentAbove(int minimum)
        {
            return Binary("currentAbove", minimum);
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList followed by an integer N. Out of all metrics passed, draws only the metrics whose value is below N at the end of the time period specified.
        /// </summary>
        public SeriesListFunction CurrentBelow(int maximum)
        {
            return Binary("currentBelow", maximum);
        }

        /// <summary>
        /// This shifts all samples later by an integer number of steps. This can be used for custom derivative calculations, among other things. Note: this will pad the early end of the data with None for every step shifted.<para/>
        /// This complements other time-displacement functions such as timeShift and timeSlice, in that this function is indifferent about the step intervals being shifted.
        /// </summary>
        public SeriesListFunction Delay(int steps)
        {
            return Binary("delay", steps);
        }

        /// <summary>
        /// This is the opposite of the integral function. This is useful for taking a running total metric and calculating the delta between subsequent data points.<para/>
        /// This function does not normalize for periods of time, as a true derivative would. Instead, see the perSecond() function to calculate a rate of change over time.
        /// </summary>
        /// <returns></returns>
        public SeriesListFunction Derivative()
        {
            return Unary("derivative");
        }

        /// <summary>
        /// Subtracts series 2 through n from series 1.
        /// </summary>
        /// <returns></returns>
        public SeriesListFunction DiffSeries(params SeriesListBase[] series)
        {
            return new SeriesListFunction("diffSeries", Merge(this, series));
        }

        /// <summary>
        /// Takes a dividend metric and a divisor metric and draws the division result.<para/>
        /// A constant may not be passed. To divide by a constant, use the scale() function
        /// (which is essentially a multiplication operation) and use the inverse of the dividend.
        /// (Division by 8 = multiplication by 1/8 or 0.125)
        /// </summary>
        /// <param name="divisorSeries"></param>
        /// <returns></returns>
        public SeriesListFunction DivideSeries(SeriesListBase divisorSeries)
        {
            return Binary("divideSeries", divisorSeries);
        }

        /// <summary>
        /// Takes a metric or a wildcard seriesList, followed by a regular expression in double quotes. Excludes metrics that match the regular expression.
        /// </summary>
        public SeriesListFunction Exclude(string pattern)
        {
            return Binary("exclude", DoubleQuote(pattern));
        }

        /// <summary>
        /// Takes a wildcard seriesList, and a second fallback metric. If the wildcard does not match any series, draws the fallback metric.
        /// </summary>
        public SeriesListFunction FallbackSeries(SeriesListBase fallback)
        {
            return Binary("fallbackSeries", fallback);
        }

        /// <summary>
        /// Takes a metric or a wildcard seriesList, followed by a regular expression in double quotes. Excludes metrics that don’t match the regular expression.
        /// </summary>
        public SeriesListFunction Grep(string pattern)
        {
            return Binary("grep", DoubleQuote(pattern));
        }

        /// <summary>
        /// Takes an arbitrary number of seriesLists and adds them to a single seriesList. This is used to pass multiple seriesLists to a function which only takes one
        /// </summary>
        public SeriesListFunction Group(params SeriesListBase[] series)
        {
            return new SeriesListFunction("group", Merge(this, series));
        }

        /// <summary>
        /// Takes a serieslist and maps a callback to subgroups within as defined by a common node
        /// </summary>
        public SeriesListFunction GroupByNode(int node, Func<SeriesListBase,Func<SeriesListFunction>> callback)
        {
            var result = callback(new GraphitePath("unused")).Invoke();
            return Ternary("groupByNode", node, DoubleQuote(result.FunctionName));
        }

        /// <summary>
        /// Takes a serieslist and maps a callback to subgroups within as defined by multiple nodes
        /// </summary>
        public SeriesListFunction GroupByNodes(Func<SeriesListBase,Func<SeriesListFunction>> callback, params int[] nodes)
        {
            var result = callback(new GraphitePath("unused")).Invoke();
            return new SeriesListFunction("groupByNodes", Merge(this, Merge(DoubleQuote(result.FunctionName), nodes)));
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList followed by an integer N. Out of all metrics passed, draws only the top N metrics with the highest average value for the time period specified.
        /// </summary>
        public SeriesListFunction HighestAverage(int number)
        {
            return Binary("highestAverage", number);
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList followed by an integer N. Out of all metrics passed, draws only the N metrics with the highest value at the end of the time period specified.
        /// </summary>
        public SeriesListFunction HighestCurrent(int number)
        {
            return Binary("highestCurrent", number);
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList followed by an integer N.<para/>
        /// Out of all metrics passed, draws only the N metrics with the highest maximum value in the time period specified.
        /// </summary>
        public SeriesListFunction HighestMax(int number)
        {
            return Binary("highestMax", number);
        }

        /// <summary>
        /// Estimate hit counts from a list of time series.<para/>
        /// This function assumes the values in each time series represent hits per second. It calculates hits per some larger interval such as per day or per hour. This function is like summarize(), except that it compensates automatically for different time scales (so that a similar graph results from using either fine-grained or coarse-grained records) and handles rarely-occurring events gracefully.
        /// </summary>
        public SeriesListFunction Hitcount(string intervalString, bool alignToInterval=false)
        {
            return Ternary("hitcount", DoubleQuote(intervalString), alignToInterval?1:0);
        }

        /// <summary>
        /// Performs a Holt-Winters forecast using the series as input data and plots the positive or negative deviation of the series data from the forecast.
        /// </summary>
        public SeriesListFunction HoltWintersAberration(int delta = 3)
        {
            return Binary("holtWintersAberration", delta);
        }

        /// <summary>
        /// Performs a Holt-Winters forecast using the series as input data and plots the area between the upper and lower bands of the predicted forecast deviations.
        /// </summary>
        public SeriesListFunction HoltWintersConfidenceArea(int delta = 3)
        {
            return Binary("holtWintersConfidenceArea", delta);
        }

        /// <summary>
        /// Performs a Holt-Winters forecast using the series as input data and plots upper and lower bands with the predicted forecast deviations.
        /// </summary>
        public SeriesListFunction HoltWintersConfidenceBands(int delta = 3)
        {
            return Binary("holtWintersConfidenceBands", delta);
        }

        /// <summary>
        /// Performs a Holt-Winters forecast using the series as input data. Data from one week previous to the series is used to bootstrap the initial forecast.
        /// </summary>
        public SeriesListFunction HoltWintersForecast()
        {
            return Unary("holtWintersForecast");
        }

        /// <summary>
        /// This will show the sum over time, sort of like a continuous addition function. Useful for finding totals or trends in metrics that are collected per minute.
        /// </summary>
        /// <returns></returns>
        public SeriesListFunction Integral()
        {
            return Unary("integral");
        }

        /// <summary>
        /// This will do the same as integral() function, except resetting the total to 0 at the given time in the parameter “from”. Useful for finding totals per hour/day/week/..
        /// </summary>
        public SeriesListFunction IntegralByInterval(string intervalUnit)
        {
            return Binary("integralByInterval", DoubleQuote(intervalUnit));
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList, and optionally a limit to the number of ‘None’ values to skip over. Continues the line with the last received value when gaps (‘None’ values) appear in your data, rather than breaking your line.
        /// </summary>
        /// <param name="limit">-1 for infinity</param>
        /// <returns></returns>
        public SeriesListFunction Interpolate(int limit = -1)
        {
            if (limit < 0)
            {
                return Unary("interpolate");
            }
            return Binary("interpolate", limit);
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList, and inverts each datapoint (i.e. 1/x).
        /// </summary>
        public SeriesListFunction Invert()
        {
            return Unary("invert");
        }

        /// <summary>
        /// Takes a metric or wildcard seriesList and counts up the number of non-null values. This is useful for understanding the number of metrics that have data at a given point in time (i.e. to count which servers are alive).
        /// </summary>
        public SeriesListFunction IsNonNull()
        {
            return Unary("isNonNull");
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList, and optionally a limit to the number of ‘None’ values to skip over. Continues the line with the last received value when gaps (‘None’ values) appear in your data, rather than breaking your line.
        /// </summary>
        public SeriesListFunction KeepLastValue(int limit = -1)
        {
            if (limit < 0)
            {
                return Unary("keepLastValue");
            }
            return Binary("keepLastValue", limit);
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList followed by an integer N.<para/>
        /// Only draw the first N metrics. Useful when testing a wildcard in a metric.
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public SeriesListFunction Limit(int number)
        {
            return Binary("limit", number);
        }

        /// <summary>
        /// Returns factor and offset of linear regression function by least squares method.
        /// </summary>
        public SeriesListFunction LinearRegressionAnalysis()
        {
            return Unary("linearRegressionAnalysis");
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList, a base, and draws the y-axis in logarithmic format. If base is omitted, the function defaults to base 10.
        /// </summary>
        public SeriesListFunction Logarithm(int logBase=10)
        {
            if (logBase == 10)
                return Unary("logarithm");
            return Binary("logarithm", logBase);
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList followed by an integer N. Out of all metrics passed, draws only the bottom N metrics with the lowest average value for the time period specified.
        /// </summary>
        public SeriesListFunction LowestAverage(int number)
        {
            return Binary("lowestAverage", number);
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList followed by an integer N. Out of all metrics passed, draws only the N metrics with the lowest value at the end of the time period specified.
        /// </summary>
        public SeriesListFunction LowestCurrent(int number)
        {
            return Binary("lowestCurrent", number);
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList. For each datapoint from each metric passed in, pick the maximum value and graph it.
        /// </summary>
        public SeriesListFunction MaxSeries()
        {
            return Unary("maxSeries");
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList. For each datapoint from each metric passed in, pick the maximum value and graph it.
        /// </summary>
        public SeriesListFunction MaxSeries(params SeriesListBase[] series)
        {
            return new SeriesListFunction("maxSeries", Merge(this, series));
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList followed by a constant n. Draws only the metrics with a maximum value above n.
        /// </summary>
        public SeriesListFunction MaximumAbove(double value)
        {
            return Binary("maximumAbove", value.ToString("r", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList followed by a constant n. Draws only the metrics with a maximum value below n.
        /// </summary>
        public SeriesListFunction MaximumBelow(double value)
        {
            return Binary("maximumBelow", value.ToString("r", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList. For each datapoint from each metric passed in, pick the minimum value and graph it.
        /// </summary>
        public SeriesListFunction MinSeries()
        {
            return Unary("minSeries");
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList. For each datapoint from each metric passed in, pick the minimum value and graph it.
        /// </summary>
        public SeriesListFunction MinSeries(params SeriesListBase[] series)
        {
            return new SeriesListFunction("minSeries", Merge(this, series));
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList followed by a constant n. Draws only the metrics with a minimum value above n.
        /// </summary>
        public SeriesListFunction MinimumAbove(double value)
        {
            return Binary("minimumAbove", value.ToString("r", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList followed by a constant n. Draws only the metrics with a minimum value below n.
        /// </summary>
        public SeriesListFunction MinimumBelow(double value)
        {
            return Binary("minimumBelow", value.ToString("r", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList followed by an integer N. Draws the N most deviant metrics. To find the deviants, the standard deviation (sigma) of each series is taken and ranked. The top N standard deviations are returned.
        /// </summary>
        public SeriesListFunction MostDeviant(int limit)
        {
            return Binary("mostDeviant", limit);
        }

        /// <summary>
        /// Graphs the moving average of a metric (or metrics) over a fixed number of past points, or a time interval.<para/>
        /// Takes one metric or a wildcard seriesList followed by a number N of datapoints. Graphs the average of the preceeding datapoints for each point on the graph.
        /// </summary>
        public SeriesListFunction MovingAverage(int windowSize)
        {
            return Binary("movingAverage", windowSize);
        }

        /// <summary>
        /// Graphs the moving average of a metric (or metrics) over a fixed number of past points, or a time interval.<para/>
        /// Takes one metric or a wildcard seriesList followed by a quoted string with a length of time like ‘1hour’ or ‘5min’ (See from / until in the render_api_ for examples of time formats). Graphs the average of the preceeding datapoints for each point on the graph.
        /// </summary>
        /// <returns></returns>
        public SeriesListFunction MovingAverage(string windowSize)
        {
            return Binary("movingAverage", SingleQuote(windowSize));
        }

        /// <summary>
        /// Graphs the moving median of a metric (or metrics) over a fixed number of past points, or a time interval.<para/>
        /// Takes one metric or a wildcard seriesList followed by a number N of datapoints. Graphs the median of the preceeding datapoints for each point on the graph.
        /// </summary>
        public SeriesListFunction MovingMedian(int windowSize)
        {
            return Binary("movingMedian", windowSize);
        }

        /// <summary>
        /// Graphs the moving median of a metric (or metrics) over a fixed number of past points, or a time interval.<para/>
        /// Takes one metric or a wildcard seriesList followed by a quoted string with a length of time like ‘1hour’ or ‘5min’ (See from / until in the render_api_ for examples of time formats). Graphs the median of the preceeding datapoints for each point on the graph.
        /// </summary>
        /// <returns></returns>
        public SeriesListFunction MovingMedian(string windowSize)
        {
            return Binary("movingMedian", SingleQuote(windowSize));
        }

        /// <summary>
        /// Takes two or more series and multiplies their points. A constant may not be used. To multiply by a constant, use the scale() function.
        /// </summary>
        public SeriesListFunction MultiplySeries()
        {
            return Unary("multiplySeries");
        }

        /// <summary>
        /// Call multiplySeries after inserting wildcards at the given position(s).
        /// </summary>
        public SeriesListFunction MultiplySeriesWithWildcards(params int[] positions)
        {
            return new SeriesListFunction("multiplySeriesWithWildcards", Merge(this, positions));
        }

        /// <summary>
        /// Returns n-percent of each series in the seriesList.
        /// </summary>
        public SeriesListFunction NPercentile(int percentile)
        {
            return Binary("nPercentile", percentile);
        }

        /// <summary>
        /// Same as the derivative function above, but ignores datapoints that trend down. Useful for counters that increase for a long time, then wrap or reset. (Such as if a network interface is destroyed and recreated by unloading and re-loading a kernel module, common with USB / WiFi cards.
        /// </summary>
        public SeriesListFunction NonNegativeDerivative()
        {
            return Unary("nonNegativeDerivative");
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList followed by a constant, and adds the constant to each datapoint.
        /// </summary>
        public SeriesListFunction Offset(double factor)
        {
            return Binary("offset", factor.ToString("r", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Offsets a metric or wildcard seriesList by subtracting the minimum value in the series from each datapoint.<para/>
        /// Useful to compare different series where the values in each series may be higher or lower on average but you’re only interested in the relative difference.
        /// </summary>
        /// <returns></returns>
        public SeriesListFunction OffsetToZero()
        {
            return Unary("offsetToZero");
        }

        /// <summary>
        /// Derivative adjusted for the series time interval This is useful for taking a running total metric and showing how many requests per second were handled.
        /// </summary>
        public SeriesListFunction PerSecond()
        {
            return Unary("perSecond");
        }

        /// <summary>
        /// percentileOfSeries returns a single series which is composed of the n-percentile values taken across a wildcard series at each point.<para/>
        /// Unless interpolate is set to True, percentile values are actual values contained in one of the supplied series.
        /// </summary>
        /// <param name="percentile"></param>
        /// <param name="interpolate"></param>
        /// <returns></returns>
        public SeriesListFunction PercentileOfSeries(int percentile, bool interpolate = false)
        {
            if (interpolate)
                return Ternary("percentileOfSeries", percentile, 1);
            return Binary("percentileOfSeries", percentile);
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList followed by a constant, and raises the datapoint by the power of the constant provided at each point.
        /// </summary>
        public SeriesListFunction Pow(int factor)
        {
            return Binary("pow", factor);
        }

        /// <summary>
        /// Takes a wildcard seriesList. Distills down a set of inputs into the range of the series
        /// </summary>
        public SeriesListFunction RangeOfSeries()
        {
            return Unary("rangeOfSeries");
        }

        /// <summary>
        /// Removes data above the nth percentile from the series or list of series provided. Values above this percentile are assigned a value of None.
        /// </summary>
        public SeriesListFunction RemoveAbovePercentile(int percentile)
        {
            return Binary("removeAbovePercentile", percentile);
        }

        /// <summary>
        /// Removes data above the given threshold from the series or list of series provided. Values above this threshold are assigned a value of None.
        /// </summary>
        public SeriesListFunction RemoveAboveValue(double percentile)
        {
            return Binary("removeAboveValue", percentile.ToString("r", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Removes data below the nth percentile from the series or list of series provided. Values below this percentile are assigned a value of None.
        /// </summary>
        public SeriesListFunction RemoveBelowPercentile(int percentile)
        {
            return Binary("removeBelowPercentile", percentile);
        }

        /// <summary>
        /// Removes data below the given threshold from the series or list of series provided. Values below this threshold are assigned a value of None.
        /// </summary>
        public SeriesListFunction RemoveBelowValue(double percentile)
        {
            return Binary("removeBelowValue", percentile.ToString("r", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Removes lines that do not have a value lying in the x-percentile of all the values at a moment
        /// </summary>
        public SeriesListFunction RemoveBetweenPercentile(int percentile)
        {
            return Binary("removeBetweenPercentile", percentile);
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList. Out of all metrics passed, draws only the metrics with not empty data
        /// </summary>
        public SeriesListFunction RemoveEmptySeries()
        {
            return Unary("removeEmptySeries");
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList followed by a constant, and multiplies the datapoint by the constant provided at each point.
        /// </summary>
        public SeriesListFunction Scale(double factor)
        {
            return Binary("scale", factor.ToString("r", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList and returns “value per seconds” where seconds is a last argument to this functions.<para/>
        /// Useful in conjunction with derivative or integral function if you want to normalize its result to a known resolution for arbitrary retentions
        /// </summary>
        public SeriesListFunction ScaleToSeconds(int seconds)
        {
            return Binary("scaleToSeconds", seconds);
        }

        /// <summary>
        /// Smarter experimental version of <see cref="Summarize"/>.
        /// </summary>
        public SeriesListFunction SmartSummarize(string interval, Func<SeriesListBase, Func<SeriesListFunction>> callback)
        {
            var func = callback(new GraphitePath("unused")).Invoke();
            return Ternary("smartSummarize", SingleQuote(interval), SingleQuote(func.FunctionName));
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList.<para/>
        /// Sorts the list of metrics by the maximum value across the time period specified. Useful with the &amp;areaMode= all parameter, to keep the lowest value lines visible.
        /// </summary>
        public SeriesListFunction SortByMaxima()
        {
            return Unary("sortByMaxima");
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList.<para/>
        /// Sorts the list of metrics by the lowest value across the time period specified.
        /// </summary>
        public SeriesListFunction SortByMinima()
        {
            return Unary("sortByMinima");
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList. Sorts the list of metrics by the metric name using either alphabetical order or natural sorting. 
        /// </summary>
        /// <param name="natural">Natural sorting allows names containing numbers to be sorted more naturally, e. g.: - Alphabetical sorting: server1, server11, server12, server2 - Natural sorting: server1, server2, server11, server12</param>
        public SeriesListFunction SortByName(bool natural = false)
        {
            if (natural)
                return Binary("sortByName", 1);
            return Unary("sortByName");
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList. Draws the standard deviation of all metrics passed at each time.
        /// </summary>
        public SeriesListFunction StddevSeries()
        {
            return Unary("stddevSeries");
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList followed by an integer N.<para/>
        /// Draw the Standard Deviation of all metrics passed for the past N datapoints. If the ratio of null points in the window is greater than windowTolerance, skip the calculation. The default for windowTolerance is 0.1 (up to 10 % of points in the window can be missing). Note that if this is set to 0.0, it will cause large gaps in the output anywhere a single point is missing.
        /// </summary>
        public SeriesListFunction Stdev(int point, double windowTolerance=0.1)
        {
            if (windowTolerance < 0 || windowTolerance > 1)
                throw new ArgumentOutOfRangeException(nameof(windowTolerance));

            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (windowTolerance == 0.1)
                return Binary("stdev", point);
            return Ternary("stdev", point, windowTolerance.ToString("r", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList.<para/>
        /// Sorts the list of metrics by the sum of values across the time period specified.
        /// </summary>
        public SeriesListFunction SortByTotal()
        {
            return Unary("sortByTotal");
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList, and computes the square root of each datapoint.
        /// </summary>
        public SeriesListFunction SquareRoot()
        {
            return Unary("squareRoot");
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList followed by 1 or 2 integers.<para/>
        /// Assume that the metric name is a list or array, with each element separated by dots. Prints n - length elements of the array (if only one integer n is passed) or n - m elements of the array (if two integers n and m are passed). The list starts with element 0 and ends with element (length - 1).
        /// </summary>
        public SeriesListFunction Substr(int start=0, int stop=0)
        {
            if (start == 0)
                return Unary("substr");
            if (stop == 0)
                return Binary("substr", start);
            return Ternary("substr", start, stop);
        }

        /// <summary>
        /// This will add metrics together and return the sum at each datapoint. (See <see cref="Integral"/> for a sum over time)
        /// </summary>
        public SeriesListFunction Sum()
        {
            return Unary("sum");
        }

        /// <summary>
        /// This will add metrics together and return the sum at each datapoint. (See <see cref="Integral"/> for a sum over time)
        /// </summary>
        /// <returns></returns>
        public SeriesListFunction Sum(params SeriesListBase[] series)
        {
            return new SeriesListFunction("sum", Merge(this, series));
        }

        /// <summary>
        /// Call sumSeries after inserting wildcards at the given position(s).
        /// </summary>
        public SeriesListFunction SumSeriesWithWildcards(params int[] positions)
        {
            return new SeriesListFunction("sumSeriesWithWildcards", Merge(this, positions));
        }

        /// <summary>
        /// Summarize the data into interval buckets of a certain size.<para/>
        /// By default, the contents of each interval bucket are summed together. This is useful for counters where each increment represents a discrete event and retrieving a “per X” value requires summing all the events in that interval.<para/>
        /// Specifying ‘avg’ instead will return the mean for each bucket, which can be more useful when the value is a gauge that represents a certain value in time.<para/>
        /// ‘max’, ‘min’ or ‘last’ can also be specified.<para/>
        /// By default, buckets are calculated by rounding to the nearest interval. This works well for intervals smaller than a day. For example, 22:32 will end up in the bucket 22:00-23:00 when the interval= 1hour.<para/>
        /// Passing alignToFrom = true will instead create buckets starting at the from time. In this case, the bucket for 22:32 depends on the from time. If from = 6:30 then the 1hour bucket for 22:32 is 22:30-23:30.
        /// </summary>
        public SeriesListFunction Summarize(string interval, SummarizeFunction func, bool alignToFrom=false)
        {
            if (alignToFrom)
                return new SeriesListFunction("summarize", this, SingleQuote(interval), SingleQuote(func.ToString()), 1);
            return Ternary("summarize", SingleQuote(interval), SingleQuote(func.ToString()));
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList, followed by a quoted string with the length of time (See from / until in the render_api_ for examples of time formats).<para/>
        /// Draws the selected metrics shifted in time. If no sign is given, a minus sign( - ) is implied which will shift the metric back in time. If a plus sign( + ) is given, the metric will be shifted forward in time.
        /// Will reset the end date range automatically to the end of the base stat unless resetEnd is False. Example case is when you timeshift to last week and have the graph date range set to include a time in the future, will limit this timeshift to pretend ending at the current time. If resetEnd is False, will instead draw full range including future time.
        /// Because time is shifted by a fixed number of seconds, comparing a time period with DST to a time period without DST, and vice - versa, will result in an apparent misalignment. For example, 8am might be overlaid with 7am. To compensate for this, use the alignDST option.
        /// Useful for comparing a metric against itself at past periods or correcting data stored at an offset.
        /// </summary>
        public SeriesListFunction TimeShift(string timeShift, bool resetEnd=true, bool alignDst=false)
        {
            timeShift = SingleQuote(timeShift);
            if (resetEnd && !alignDst)
                return Binary("timeShift", timeShift);
            return new SeriesListFunction("timeShift", this, timeShift, resetEnd ? 1 : 0, alignDst ? 1 : 0);
        }

        /// <summary>
        /// Takes one metric or a wildcard metric, followed by a quoted string with the time to start the line and another quoted string with the time to end the line. The start and end times are inclusive. See from / until in the render_api_ for examples of time formats.<para/>
        /// Useful for filtering out a part of a series of data from a wider range of data.
        /// </summary>
        public SeriesListFunction TimeSlice(string startSliceAt, string endSliceAt="now")
        {
            startSliceAt = SingleQuote(startSliceAt);
            endSliceAt = SingleQuote(endSliceAt);
            if (endSliceAt == "'now'")
                return Binary("timeSlice", startSliceAt);
            return Ternary("timeSlice", startSliceAt, endSliceAt);
        }

        /// <summary>
        /// Takes one metric or a wildcard seriesList, followed by a quoted string with the length of time (See from / until in the render_api_ for examples of time formats). Also takes a start multiplier and end multiplier for the length of time<para/>
        /// create a seriesList which is composed of the original metric series stacked with time shifts starting time shifts from the start multiplier through the end multiplier
        /// Useful for looking at history, or feeding into averageSeries or stddevSeries.
        /// </summary>
        public SeriesListFunction TimeStack(string timeShiftUnit, int timeShiftStart, int timeShiftEnd)
        {
            return new SeriesListFunction("timeStack", this, SingleQuote(timeShiftUnit), timeShiftStart, timeShiftEnd);
        }

        /// <summary>
        /// Takes a metric or wildcard seriesList and replaces null values with the value specified by default. The value 0 used if not specified. The optional referenceSeries, if specified, is a metric or wildcard series list that governs which time intervals nulls should be replaced. If specified, nulls are replaced only in intervals where a non-null is found for the same interval in any of referenceSeries. This method compliments the drawNullAsZero function in graphical mode, but also works in text-only mode.
        /// </summary>
        public SeriesListFunction TransformNull(double defaultValue = 0.0,SeriesListBase referenceSeries = null)
        {
            if (referenceSeries != null)
                return Ternary("transformNull", defaultValue.ToString("r", CultureInfo.InvariantCulture),referenceSeries);
            
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (defaultValue == 0.0)
                return Unary("transformNull");
            return Binary("transformNull", defaultValue.ToString("r", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Compares the maximum of each series against the given value. If the series maximum is greater than <paramref name="value"/>, the regular expression search and replace is applied against the series name to plot a related metric<para/>
        /// e. g. given useSeriesAbove(ganglia.metric1.reqs,10,’reqs’,’time’), the response time metric will be plotted only when the maximum value of the corresponding request/s metric is > 10
        /// </summary>
        /// <param name="value"></param>
        /// <param name="search"></param>
        /// <param name="replace"></param>
        /// <returns></returns>
        public SeriesListFunction UseSeriesAbove(double value, string search, string replace)
        {
            return new SeriesListFunction("useSeriesAbove", this, value.ToString("r", CultureInfo.InvariantCulture), DoubleQuote(search), DoubleQuote(replace));
        }

        /// <summary>
        /// Takes a series of average values and a series of weights and produces a weighted average for all values. The corresponding values should share one or more zero-indexed nodes.
        /// </summary>
        public SeriesListFunction WeightedAverage(SeriesListBase seriesListWeight, params int[] nodes)
        {
            if (seriesListWeight == null)
                throw new ArgumentNullException(nameof(seriesListWeight));
            return new SeriesListFunction("weightedAverage", Merge(this, Merge(seriesListWeight, nodes)));
        }

        /// <summary>
        /// allows the metric paths to contain named variables.
        /// Variable values can be specified or overriden in <see cref="GraphiteClient.GetMetricsDataAsync(ahd.Graphite.Base.SeriesListBase,string,string,System.Collections.Generic.IDictionary{string,string},System.Nullable{ulong},System.Threading.CancellationToken)"/>
        /// </summary>
        /// <param name="defaultValues">default values for the template variables</param>
        /// <returns></returns>
        public SeriesListFunction Template(params Tuple<string, string>[] defaultValues)
        {
            return new SeriesListFunction("template", Merge(this, defaultValues.Select(x => $"{x.Item1}={DoubleQuote(x.Item2)}").ToArray()));
        }

        /// <summary>
        /// allows the metric paths to contain positional variables.
        /// Variable values can be specified or overriden in <see cref="GraphiteClient.GetMetricsDataAsync(ahd.Graphite.Base.SeriesListBase,string,string,System.Collections.Generic.IDictionary{string,string},System.Nullable{ulong},System.Threading.CancellationToken)"/>
        /// </summary>
        /// <param name="defaultValues">default values for the template variables</param>
        /// <returns></returns>
        public SeriesListFunction Template(params string[] defaultValues)
        {
            return new SeriesListFunction("template", Merge(this, defaultValues.Select(DoubleQuote).ToArray()));  
        }
    }
}
