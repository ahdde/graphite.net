using System;
using ahd.Graphite.Base;
using ahd.Graphite.Functions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ahd.Graphite.Test
{
    [TestClass]
    public class GraphiteFunctionsTest
    {
        private readonly SeriesListBase _series = new GraphitePath("metric");

        [TestMethod]
        public void Alias()
        {
            var alias = _series.Alias("Foo");
            Assert.AreEqual("alias(metric,\"Foo\")", alias.ToString());
        }

        [TestMethod]
        public void Absolute()
        {
            var absolute = _series.Absolute();
            Assert.AreEqual("absolute(metric)", absolute.ToString());
        }

        [TestMethod]
        public void AggregateLine()
        {
            var aggregateLine = _series.AggregateLine();
            Assert.AreEqual("aggregateLine(metric,'avg')", aggregateLine.ToString());
        }

        [TestMethod]
        public void AliasByMetric()
        {
            var aliasByMetric = _series.AliasByMetric();
            Assert.AreEqual("aliasByMetric(metric)", aliasByMetric.ToString());
        }

        [TestMethod]
        public void AliasByNode()
        {
            var aliasByNode = _series.AliasByNode(1, 4);
            Assert.AreEqual("aliasByNode(metric,1,4)", aliasByNode.ToString());
        }

        [TestMethod]
        public void AliasSub()
        {
            var aliasSub = _series.AliasSub("^.*TCP(\\d+)", "\\1");
            Assert.AreEqual("aliasSub(metric,\"^.*TCP(\\d+)\",\"\\1\")", aliasSub.ToString());
        }

        [TestMethod]
        public void ApplyByNode()
        {
            var innerCalc = GraphitePath.Parse("%.disk.bytes_free")
                .DivideSeries(GraphitePath.Parse("%.disk.bytes_*").SumSeriesWithWildcards());
            var applyByNode = GraphitePath.Parse("servers.*.disk.bytes_free").ApplyByNode(1, innerCalc);
            Assert.AreEqual("applyByNode(servers.*.disk.bytes_free,1,\"divideSeries(%.disk.bytes_free,sumSeriesWithWildcards(%.disk.bytes_*))\")", applyByNode.ToString());
        }

        [TestMethod]
        public void AsPercent()
        {
            var asPercent = _series.AsPercent(1500);
            Assert.AreEqual("asPercent(metric,1500)", asPercent.ToString());
            asPercent = _series.AsPercent(_series);
            Assert.AreEqual("asPercent(metric,metric)", asPercent.ToString());
            asPercent = _series.AsPercent();
            Assert.AreEqual("asPercent(metric)", asPercent.ToString());
        }

        [TestMethod]
        public void AverageAbove()
        {
            var averageAbove = _series.AverageAbove(5);
            Assert.AreEqual("averageAbove(metric,5)", averageAbove.ToString());
        }

        [TestMethod]
        public void AverageBelow()
        {
            var averageBelow = _series.AverageBelow(5);
            Assert.AreEqual("averageBelow(metric,5)", averageBelow.ToString());
        }

        [TestMethod]
        public void AverageOutsidePercentile()
        {
            var series = _series.AverageOutsidePercentile(5);
            Assert.AreEqual("averageOutsidePercentile(metric,5)", series.ToString());
        }

        [TestMethod]
        public void Avg()
        {
            var series = _series.Avg();
            Assert.AreEqual("avg(metric)", series.ToString());
        }

        [TestMethod]
        public void AverageSeriesWithWildcards()
        {
            var series = _series.AverageSeriesWithWildcards(1, 5);
            Assert.AreEqual("averageSeriesWithWildcards(metric,1,5)", series.ToString());
        }

        [TestMethod]
        public void Changed()
        {
            var series = _series.Changed();
            Assert.AreEqual("changed(metric)", series.ToString());
        }

        [TestMethod]
        public void ConsolidateBy()
        {
            var series = _series.ConsolidateBy(ConsolidateFunction.max);
            Assert.AreEqual("consolidateBy(metric,'max')", series.ToString());
        }

        [TestMethod]
        public void CountSeries()
        {
            var series = _series.CountSeries();
            Assert.AreEqual("countSeries(metric)", series.ToString());
        }

        [TestMethod]
        public void CurrentAbove()
        {
            var series = _series.CurrentAbove(5);
            Assert.AreEqual("currentAbove(metric,5)", series.ToString());
        }

        [TestMethod]
        public void CurrentBelow()
        {
            var series = _series.CurrentBelow(5);
            Assert.AreEqual("currentBelow(metric,5)", series.ToString());
        }

        [TestMethod]
        public void Delay()
        {
            var series = _series.Delay(5);
            Assert.AreEqual("delay(metric,5)", series.ToString());
        }

        [TestMethod]
        public void Derivative()
        {
            var series = _series.Derivative();
            Assert.AreEqual("derivative(metric)", series.ToString());
        }

        [TestMethod]
        public void DiffSeries()
        {
            var series = _series.DiffSeries(_series);
            Assert.AreEqual("diffSeries(metric,metric)", series.ToString());
        }

        [TestMethod]
        public void DivideSeries()
        {
            var series = _series.DivideSeries(_series);
            Assert.AreEqual("divideSeries(metric,metric)", series.ToString());
        }

        [TestMethod]
        public void Exclude()
        {
            var series = _series.Exclude("server02");
            Assert.AreEqual("exclude(metric,\"server02\")", series.ToString());
        }

        [TestMethod]
        public void FallbackSeries()
        {
            var series = _series.FallbackSeries(new ConstantLineSeriesList(5));
            Assert.AreEqual("fallbackSeries(metric,constantLine(5))", series.ToString());
        }

        [TestMethod]
        public void Grep()
        {
            var series = _series.Grep("server02");
            Assert.AreEqual("grep(metric,\"server02\")", series.ToString());
        }

        [TestMethod]
        public void Group()
        {
            var series = _series.Group(_series, _series);
            Assert.AreEqual("group(metric,metric,metric)", series.ToString());
        }

        [TestMethod]
        public void GroupByNode()
        {
            var series = _series.GroupByNode(2, x => x.Sum);
            Assert.AreEqual("groupByNode(metric,2,\"sum\")", series.ToString());
        }

        [TestMethod]
        public void GroupByNodes()
        {
            var series = _series.GroupByNodes(x => x.Sum, 1, 4);
            Assert.AreEqual("groupByNodes(metric,\"sum\",1,4)", series.ToString());
        }

        [TestMethod]
        public void HighestAverage()
        {
            var series = _series.HighestAverage(4);
            Assert.AreEqual("highestAverage(metric,4)", series.ToString());
        }

        [TestMethod]
        public void HighestCurrent()
        {
            var series = _series.HighestCurrent(4);
            Assert.AreEqual("highestCurrent(metric,4)", series.ToString());
        }

        [TestMethod]
        public void HighestMax()
        {
            var series = _series.HighestMax(4);
            Assert.AreEqual("highestMax(metric,4)", series.ToString());
        }

        [TestMethod]
        public void Hitcount()
        {
            var series = _series.Hitcount("day", alignToInterval: false);
            Assert.AreEqual("hitcount(metric,\"day\",0)", series.ToString());
        }

        [TestMethod]
        public void HoltWintersAberration()
        {
            var series = _series.HoltWintersAberration();
            Assert.AreEqual("holtWintersAberration(metric,3)", series.ToString());
        }

        [TestMethod]
        public void HoltWintersConfidenceArea()
        {
            var series = _series.HoltWintersConfidenceArea(4);
            Assert.AreEqual("holtWintersConfidenceArea(metric,4)", series.ToString());
        }

        [TestMethod]
        public void HoltWintersConfidenceBands()
        {
            var series = _series.HoltWintersConfidenceBands(3);
            Assert.AreEqual("holtWintersConfidenceBands(metric,3)", series.ToString());
        }

        [TestMethod]
        public void HoltWintersForecast()
        {
            var series = _series.HoltWintersForecast();
            Assert.AreEqual("holtWintersForecast(metric)", series.ToString());
        }

        [TestMethod]
        public void Integral()
        {
            var series = _series.Integral();
            Assert.AreEqual("integral(metric)", series.ToString());
        }

        [TestMethod]
        public void IntegralByInterval()
        {
            var series = _series.IntegralByInterval("1d");
            Assert.AreEqual("integralByInterval(metric,\"1d\")", series.ToString());
        }

        [TestMethod]
        public void Interpolate()
        {
            var series = _series.Interpolate(-1);
            Assert.AreEqual("interpolate(metric)", series.ToString());
            series = _series.Interpolate();
            Assert.AreEqual("interpolate(metric)", series.ToString());
            series = _series.Interpolate(10);
            Assert.AreEqual("interpolate(metric,10)", series.ToString());

        }

        [TestMethod]
        public void Invert()
        {
            var series = _series.Invert();
            Assert.AreEqual("invert(metric)", series.ToString());
        }

        [TestMethod]
        public void IsNonNull()
        {
            var series = _series.IsNonNull();
            Assert.AreEqual("isNonNull(metric)", series.ToString());
        }

        [TestMethod]
        public void KeepLastValue()
        {
            var series = _series.KeepLastValue(-1);
            Assert.AreEqual("keepLastValue(metric)", series.ToString());
            series = _series.KeepLastValue();
            Assert.AreEqual("keepLastValue(metric)", series.ToString());
            series = _series.KeepLastValue(10);
            Assert.AreEqual("keepLastValue(metric,10)", series.ToString());
        }

        [TestMethod]
        public void Limit()
        {
            var series = _series.Limit(10);
            Assert.AreEqual("limit(metric,10)", series.ToString());
        }

        [TestMethod]
        public void LinearRegressionAnalysis()
        {
            var series = _series.LinearRegressionAnalysis();
            Assert.AreEqual("linearRegressionAnalysis(metric)", series.ToString());
        }

        [TestMethod]
        public void Logarithm()
        {
            var series = _series.Logarithm(10);
            Assert.AreEqual("logarithm(metric)", series.ToString());
            series = _series.Logarithm();
            Assert.AreEqual("logarithm(metric)", series.ToString());
            series = _series.Logarithm(2);
            Assert.AreEqual("logarithm(metric,2)", series.ToString());
        }

        [TestMethod]
        public void LowestAverage()
        {
            var series = _series.LowestAverage(10);
            Assert.AreEqual("lowestAverage(metric,10)", series.ToString());
        }

        [TestMethod]
        public void LowestCurrent()
        {
            var series = _series.LowestCurrent(10);
            Assert.AreEqual("lowestCurrent(metric,10)", series.ToString());
        }

        [TestMethod]
        public void MaxSeries()
        {
            var series = _series.MaxSeries();
            Assert.AreEqual("maxSeries(metric)", series.ToString());
        }

        [TestMethod]
        public void MaximumAbove()
        {
            var series = _series.MaximumAbove(10);
            Assert.AreEqual("maximumAbove(metric,10)", series.ToString());
        }

        [TestMethod]
        public void MaximumBelow()
        {
            var series = _series.MaximumBelow(10);
            Assert.AreEqual("maximumBelow(metric,10)", series.ToString());
        }

        [TestMethod]
        public void MinSeries()
        {
            var series = _series.MinSeries();
            Assert.AreEqual("minSeries(metric)", series.ToString());
        }

        [TestMethod]
        public void MinimumAbove()
        {
            var series = _series.MinimumAbove(10);
            Assert.AreEqual("minimumAbove(metric,10)", series.ToString());
        }

        [TestMethod]
        public void MinimumBelow()
        {
            var series = _series.MinimumBelow(10);
            Assert.AreEqual("minimumBelow(metric,10)", series.ToString());
        }

        [TestMethod]
        public void MostDeviant()
        {
            var series = _series.MostDeviant(10);
            Assert.AreEqual("mostDeviant(metric,10)", series.ToString());
        }

        [TestMethod]
        public void MovingAverage()
        {
            var series = _series.MovingAverage(10);
            Assert.AreEqual("movingAverage(metric,10)", series.ToString());
            series = _series.MovingAverage("5min");
            Assert.AreEqual("movingAverage(metric,'5min')", series.ToString());
        }

        [TestMethod]
        public void MovingMedian()
        {
            var series = _series.MovingMedian(10);
            Assert.AreEqual("movingMedian(metric,10)", series.ToString());
            series = _series.MovingMedian("5min");
            Assert.AreEqual("movingMedian(metric,'5min')", series.ToString());
        }

        [TestMethod]
        public void MultiplySeries()
        {
            var series = _series.MultiplySeries();
            Assert.AreEqual("multiplySeries(metric)", series.ToString());
        }

        [TestMethod]
        public void MultiplySeriesWithWildcards()
        {
            var series = _series.MultiplySeriesWithWildcards(1, 5);
            Assert.AreEqual("multiplySeriesWithWildcards(metric,1,5)", series.ToString());
        }

        [TestMethod]
        public void NPercentile()
        {
            var series = _series.NPercentile(10);
            Assert.AreEqual("nPercentile(metric,10)", series.ToString());
        }

        [TestMethod]
        public void NonNegativeDerivative()
        {
            var series = _series.NonNegativeDerivative();
            Assert.AreEqual("nonNegativeDerivative(metric)", series.ToString());
        }

        [TestMethod]
        public void Offset()
        {
            var series = _series.Offset(10);
            Assert.AreEqual("offset(metric,10)", series.ToString());
        }

        [TestMethod]
        public void OffsetToZero()
        {
            var series = _series.OffsetToZero();
            Assert.AreEqual("offsetToZero(metric)", series.ToString());
        }

        [TestMethod]
        public void PerSecond()
        {
            var series = _series.PerSecond();
            Assert.AreEqual("perSecond(metric)", series.ToString());
        }

        [TestMethod]
        public void PercentileOfSeries()
        {
            var series = _series.PercentileOfSeries(10, false);
            Assert.AreEqual("percentileOfSeries(metric,10)", series.ToString());
            series = _series.PercentileOfSeries(10, true);
            Assert.AreEqual("percentileOfSeries(metric,10,1)", series.ToString());
        }

        [TestMethod]
        public void Pow()
        {
            var series = _series.Pow(10);
            Assert.AreEqual("pow(metric,10)", series.ToString());
        }

        [TestMethod]
        public void RangeOfSeries()
        {
            var series = _series.RangeOfSeries();
            Assert.AreEqual("rangeOfSeries(metric)", series.ToString());
        }

        [TestMethod]
        public void RemoveAbovePercentile()
        {
            var series = _series.RemoveAbovePercentile(10);
            Assert.AreEqual("removeAbovePercentile(metric,10)", series.ToString());
        }

        [TestMethod]
        public void RemoveAboveValue()
        {
            var series = _series.RemoveAboveValue(10);
            Assert.AreEqual("removeAboveValue(metric,10)", series.ToString());
        }

        [TestMethod]
        public void RemoveBelowPercentile()
        {
            var series = _series.RemoveBelowPercentile(10);
            Assert.AreEqual("removeBelowPercentile(metric,10)", series.ToString());
        }

        [TestMethod]
        public void RemoveBelowValue()
        {
            var series = _series.RemoveBelowValue(10);
            Assert.AreEqual("removeBelowValue(metric,10)", series.ToString());
        }

        [TestMethod]
        public void RemoveBetweenPercentile()
        {
            var series = _series.RemoveBetweenPercentile(10);
            Assert.AreEqual("removeBetweenPercentile(metric,10)", series.ToString());
        }

        [TestMethod]
        public void RemoveEmptySeries()
        {
            var series = _series.RemoveEmptySeries();
            Assert.AreEqual("removeEmptySeries(metric)", series.ToString());
        }

        [TestMethod]
        public void Scale()
        {
            var series = _series.Scale(10);
            Assert.AreEqual("scale(metric,10)", series.ToString());
        }

        [TestMethod]
        public void ScaleToSeconds()
        {
            var series = _series.ScaleToSeconds(10);
            Assert.AreEqual("scaleToSeconds(metric,10)", series.ToString());
        }

        [TestMethod]
        public void SmartSummarize()
        {
            var series = _series.SmartSummarize("1day", x => x.Sum);
            Assert.AreEqual("smartSummarize(metric,'1day','sum')", series.ToString());
        }

        [TestMethod]
        public void SortByMaxima()
        {
            var series = _series.SortByMaxima();
            Assert.AreEqual("sortByMaxima(metric)", series.ToString());
        }

        [TestMethod]
        public void SortByMinima()
        {
            var series = _series.SortByMinima();
            Assert.AreEqual("sortByMinima(metric)", series.ToString());
        }

        [TestMethod]
        public void SortByName()
        {
            var series = _series.SortByName();
            Assert.AreEqual("sortByName(metric)", series.ToString());
            series = _series.SortByName(true);
            Assert.AreEqual("sortByName(metric,1)", series.ToString());
        }

        [TestMethod]
        public void SortByTotal()
        {
            var series = _series.SortByTotal();
            Assert.AreEqual("sortByTotal(metric)", series.ToString());
        }

        [TestMethod]
        public void SquareRoot()
        {
            var series = _series.SquareRoot();
            Assert.AreEqual("squareRoot(metric)", series.ToString());
        }

        [TestMethod]
        public void StddevSeries()
        {
            var series = _series.StddevSeries();
            Assert.AreEqual("stddevSeries(metric)", series.ToString());
        }

        [TestMethod]
        public void Stdev()
        {
            var series = _series.Stdev(1, 0.3);
            Assert.AreEqual("stdev(metric,1,0.3)", series.ToString());
            series = _series.Stdev(1);
            Assert.AreEqual("stdev(metric,1)", series.ToString());
        }

        [TestMethod]
        public void Substr()
        {
            var series = _series.Substr(2, 4);
            Assert.AreEqual("substr(metric,2,4)", series.ToString());
        }

        [TestMethod]
        public void Sum()
        {
            var series = _series.Sum();
            Assert.AreEqual("sum(metric)", series.ToString());

            var sumSeries = _series.Sum(new GraphitePath("test1"), new GraphitePath("test2"));
            Assert.AreEqual("sum(metric,test1,test2)", sumSeries.ToString());

            var serie1 = _series.ConsolidateBy(ConsolidateFunction.max);
            var serie2 = _series.ConsolidateBy(ConsolidateFunction.sum);
            Assert.AreEqual("sum(consolidateBy(metric,'max'),consolidateBy(metric,'sum'))", new SeriesListBase[] {serie1, serie2}.Sum().ToString());
        }

        [TestMethod]
        public void SumSeriesWithWildcards()
        {
            var series = _series.SumSeriesWithWildcards(1, 4);
            Assert.AreEqual("sumSeriesWithWildcards(metric,1,4)", series.ToString());
        }

        [TestMethod]
        public void Summarize()
        {
            var series = _series.Summarize("1day", SummarizeFunction.sum);
            Assert.AreEqual("summarize(metric,'1day','sum')", series.ToString());
        }

        [TestMethod]
        public void TimeShift()
        {
            var series = _series.TimeShift("1day", resetEnd: true, alignDst: false);
            Assert.AreEqual("timeShift(metric,'1day')", series.ToString());
            series = _series.TimeShift("1day", resetEnd: false, alignDst: true);
            Assert.AreEqual("timeShift(metric,'1day',0,1)", series.ToString());
        }

        [TestMethod]
        public void TimeSlice()
        {
            var series = _series.TimeSlice("-7day", "-1day");
            Assert.AreEqual("timeSlice(metric,'-7day','-1day')", series.ToString());
            series = _series.TimeSlice("-7day");
            Assert.AreEqual("timeSlice(metric,'-7day')", series.ToString());
            series = _series.TimeSlice("-7day", "now");
            Assert.AreEqual("timeSlice(metric,'-7day')", series.ToString());
        }

        [TestMethod]
        public void TimeStack()
        {
            var series = _series.TimeStack("-7day", 0, 7);
            Assert.AreEqual("timeStack(metric,'-7day',0,7)", series.ToString());
        }

        [TestMethod]
        public void TransformNull()
        {
            var series = _series.TransformNull();
            Assert.AreEqual("transformNull(metric)", series.ToString());
            series = _series.TransformNull(0);
            Assert.AreEqual("transformNull(metric)", series.ToString());
            series = _series.TransformNull(10);
            Assert.AreEqual("transformNull(metric,10)", series.ToString());
            series = _series.TransformNull(10, _series);
            Assert.AreEqual("transformNull(metric,10,metric)", series.ToString());
        }

        [TestMethod]
        public void UseSeriesAbove()
        {
            var series = _series.UseSeriesAbove(10, "reqs", "time");
            Assert.AreEqual("useSeriesAbove(metric,10,\"reqs\",\"time\")", series.ToString());
        }

        [TestMethod]
        public void WeightedAverage()
        {
            var series = _series.WeightedAverage(_series, 1, 3, 4);
            Assert.AreEqual("weightedAverage(metric,metric,1,3,4)", series.ToString());
        }
    }
}