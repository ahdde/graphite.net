using System;
using System.Collections.Generic;
using ahd.Graphite.Base;
using ahd.Graphite.Functions;
using Xunit;

namespace Graphite.Core.Tests
{
    public class GraphiteFunctionsTest
    {
        private readonly SeriesListBase _series = new GraphitePath("metric");

        [Fact]
        public void Alias()
        {
            var alias = _series.Alias("Foo");
            Assert.Equal("alias(metric,\"Foo\")", alias.ToString());
        }

        [Fact]
        public void Absolute()
        {
            var absolute = _series.Absolute();
            Assert.Equal("absolute(metric)", absolute.ToString());
        }

        [Fact]
        public void AggregateLine()
        {
            var aggregateLine = _series.AggregateLine();
            Assert.Equal("aggregateLine(metric,'avg')", aggregateLine.ToString());
        }

        [Fact]
        public void AliasByMetric()
        {
            var aliasByMetric = _series.AliasByMetric();
            Assert.Equal("aliasByMetric(metric)", aliasByMetric.ToString());
        }

        [Fact]
        public void AliasByNode()
        {
            var aliasByNode = _series.AliasByNode(1, 4);
            Assert.Equal("aliasByNode(metric,1,4)", aliasByNode.ToString());
        }

        [Fact]
        public void AliasSub()
        {
            var aliasSub = _series.AliasSub("^.*TCP(\\d+)", "\\1");
            Assert.Equal("aliasSub(metric,\"^.*TCP(\\d+)\",\"\\1\")", aliasSub.ToString());
        }

        [Fact]
        public void ApplyByNode()
        {
            var innerCalc = GraphitePath.Parse("%.disk.bytes_free")
                .DivideSeries(GraphitePath.Parse("%.disk.bytes_*").SumSeriesWithWildcards());
            var applyByNode = GraphitePath.Parse("servers.*.disk.bytes_free").ApplyByNode(1, innerCalc);
            Assert.Equal("applyByNode(servers.*.disk.bytes_free,1,\"divideSeries(%.disk.bytes_free,sumSeriesWithWildcards(%.disk.bytes_*))\")", applyByNode.ToString());
        }

        [Fact]
        public void AsPercent()
        {
            var asPercent = _series.AsPercent(1500);
            Assert.Equal("asPercent(metric,1500)", asPercent.ToString());
            asPercent = _series.AsPercent(_series);
            Assert.Equal("asPercent(metric,metric)", asPercent.ToString());
            asPercent = _series.AsPercent();
            Assert.Equal("asPercent(metric)", asPercent.ToString());
        }

        [Fact]
        public void AverageAbove()
        {
            var averageAbove = _series.AverageAbove(5);
            Assert.Equal("averageAbove(metric,5)", averageAbove.ToString());
            averageAbove = _series.AverageAbove(1.1);
            Assert.Equal("averageAbove(metric,1.1)", averageAbove.ToString());
        }

        [Fact]
        public void AverageBelow()
        {
            var averageBelow = _series.AverageBelow(5);
            Assert.Equal("averageBelow(metric,5)", averageBelow.ToString());
            averageBelow = _series.AverageBelow(1.1);
            Assert.Equal("averageBelow(metric,1.1)", averageBelow.ToString());
        }

        [Fact]
        public void AverageOutsidePercentile()
        {
            var series = _series.AverageOutsidePercentile(5);
            Assert.Equal("averageOutsidePercentile(metric,5)", series.ToString());
        }

        [Fact]
        public void Avg()
        {
            var series = _series.Avg();
            Assert.Equal("avg(metric)", series.ToString());
        }

        [Fact]
        public void AverageSeriesWithWildcards()
        {
            var series = _series.AverageSeriesWithWildcards(1, 5);
            Assert.Equal("averageSeriesWithWildcards(metric,1,5)", series.ToString());
        }

        [Fact]
        public void Changed()
        {
            var series = _series.Changed();
            Assert.Equal("changed(metric)", series.ToString());
        }

        [Fact]
        public void ConsolidateBy()
        {
            var series = _series.ConsolidateBy(ConsolidateFunction.max);
            Assert.Equal("consolidateBy(metric,'max')", series.ToString());
        }

        [Fact]
        public void CountSeries()
        {
            var series = _series.CountSeries();
            Assert.Equal("countSeries(metric)", series.ToString());
        }

        [Fact]
        public void CurrentAbove()
        {
            var series = _series.CurrentAbove(5);
            Assert.Equal("currentAbove(metric,5)", series.ToString());
        }

        [Fact]
        public void CurrentBelow()
        {
            var series = _series.CurrentBelow(5);
            Assert.Equal("currentBelow(metric,5)", series.ToString());
        }

        [Fact]
        public void Delay()
        {
            var series = _series.Delay(5);
            Assert.Equal("delay(metric,5)", series.ToString());
        }

        [Fact]
        public void Derivative()
        {
            var series = _series.Derivative();
            Assert.Equal("derivative(metric)", series.ToString());
        }

        [Fact]
        public void DiffSeries()
        {
            var series = _series.DiffSeries(_series);
            Assert.Equal("diffSeries(metric,metric)", series.ToString());
        }

        [Fact]
        public void DivideSeries()
        {
            var series = _series.DivideSeries(_series);
            Assert.Equal("divideSeries(metric,metric)", series.ToString());
        }

        [Fact]
        public void Exclude()
        {
            var series = _series.Exclude("server02");
            Assert.Equal("exclude(metric,\"server02\")", series.ToString());
        }

        [Fact]
        public void FallbackSeries()
        {
            var series = _series.FallbackSeries(new ConstantLineSeriesList(5));
            Assert.Equal("fallbackSeries(metric,constantLine(5))", series.ToString());
        }

        [Fact]
        public void Grep()
        {
            var series = _series.Grep("server02");
            Assert.Equal("grep(metric,\"server02\")", series.ToString());
        }

        [Fact]
        public void Group()
        {
            var series = _series.Group();
            Assert.Equal("group(metric)", series.ToString());

            var sumSeries = _series.Group(new GraphitePath("test1"), _series);
            Assert.Equal("group(metric,test1,metric)", sumSeries.ToString());

            var serie1 = _series.ConsolidateBy(ConsolidateFunction.max);
            var serie2 = _series.ConsolidateBy(ConsolidateFunction.sum);
            Assert.Equal("group(consolidateBy(metric,'max'),consolidateBy(metric,'sum'))", new SeriesListBase[] {serie1, serie2}.Group().ToString());

            IEnumerable<SeriesListBase> sources = new[] { _series };
            Assert.Equal("group(metric)", sources.Group().ToString());
        }

        [Fact]
        public void GroupByNode()
        {
            var series = _series.GroupByNode(2, x => x.Sum);
            Assert.Equal("groupByNode(metric,2,\"sum\")", series.ToString());
        }

        [Fact]
        public void GroupByNodes()
        {
            var series = _series.GroupByNodes(x => x.Sum, 1, 4);
            Assert.Equal("groupByNodes(metric,\"sum\",1,4)", series.ToString());
        }

        [Fact]
        public void HighestAverage()
        {
            var series = _series.HighestAverage(4);
            Assert.Equal("highestAverage(metric,4)", series.ToString());
        }

        [Fact]
        public void HighestCurrent()
        {
            var series = _series.HighestCurrent(4);
            Assert.Equal("highestCurrent(metric,4)", series.ToString());
        }

        [Fact]
        public void HighestMax()
        {
            var series = _series.HighestMax(4);
            Assert.Equal("highestMax(metric,4)", series.ToString());
        }

        [Fact]
        public void Hitcount()
        {
            var series = _series.Hitcount("day", alignToInterval: false);
            Assert.Equal("hitcount(metric,\"day\",0)", series.ToString());
        }

        [Fact]
        public void HoltWintersAberration()
        {
            var series = _series.HoltWintersAberration();
            Assert.Equal("holtWintersAberration(metric,3)", series.ToString());
        }

        [Fact]
        public void HoltWintersConfidenceArea()
        {
            var series = _series.HoltWintersConfidenceArea(4);
            Assert.Equal("holtWintersConfidenceArea(metric,4)", series.ToString());
        }

        [Fact]
        public void HoltWintersConfidenceBands()
        {
            var series = _series.HoltWintersConfidenceBands(3);
            Assert.Equal("holtWintersConfidenceBands(metric,3)", series.ToString());
        }

        [Fact]
        public void HoltWintersForecast()
        {
            var series = _series.HoltWintersForecast();
            Assert.Equal("holtWintersForecast(metric)", series.ToString());
        }

        [Fact]
        public void Integral()
        {
            var series = _series.Integral();
            Assert.Equal("integral(metric)", series.ToString());
        }

        [Fact]
        public void IntegralByInterval()
        {
            var series = _series.IntegralByInterval("1d");
            Assert.Equal("integralByInterval(metric,\"1d\")", series.ToString());
        }

        [Fact]
        public void Interpolate()
        {
            var series = _series.Interpolate(-1);
            Assert.Equal("interpolate(metric)", series.ToString());
            series = _series.Interpolate();
            Assert.Equal("interpolate(metric)", series.ToString());
            series = _series.Interpolate(10);
            Assert.Equal("interpolate(metric,10)", series.ToString());

        }

        [Fact]
        public void Invert()
        {
            var series = _series.Invert();
            Assert.Equal("invert(metric)", series.ToString());
        }

        [Fact]
        public void IsNonNull()
        {
            var series = _series.IsNonNull();
            Assert.Equal("isNonNull(metric)", series.ToString());
        }

        [Fact]
        public void KeepLastValue()
        {
            var series = _series.KeepLastValue(-1);
            Assert.Equal("keepLastValue(metric)", series.ToString());
            series = _series.KeepLastValue();
            Assert.Equal("keepLastValue(metric)", series.ToString());
            series = _series.KeepLastValue(10);
            Assert.Equal("keepLastValue(metric,10)", series.ToString());
        }

        [Fact]
        public void Limit()
        {
            var series = _series.Limit(10);
            Assert.Equal("limit(metric,10)", series.ToString());
        }

        [Fact]
        public void LinearRegressionAnalysis()
        {
            var series = _series.LinearRegressionAnalysis();
            Assert.Equal("linearRegressionAnalysis(metric)", series.ToString());
        }

        [Fact]
        public void Logarithm()
        {
            var series = _series.Logarithm(10);
            Assert.Equal("logarithm(metric)", series.ToString());
            series = _series.Logarithm();
            Assert.Equal("logarithm(metric)", series.ToString());
            series = _series.Logarithm(2);
            Assert.Equal("logarithm(metric,2)", series.ToString());
        }

        [Fact]
        public void LowestAverage()
        {
            var series = _series.LowestAverage(10);
            Assert.Equal("lowestAverage(metric,10)", series.ToString());
        }

        [Fact]
        public void LowestCurrent()
        {
            var series = _series.LowestCurrent(10);
            Assert.Equal("lowestCurrent(metric,10)", series.ToString());
        }

        [Fact]
        public void MaxSeries()
        {
            var series = _series.MaxSeries();
            Assert.Equal("maxSeries(metric)", series.ToString());

            var sumSeries = _series.MaxSeries(new GraphitePath("test1"), new GraphitePath("test2"));
            Assert.Equal("maxSeries(metric,test1,test2)", sumSeries.ToString());

            var serie1 = _series.ConsolidateBy(ConsolidateFunction.max);
            var serie2 = _series.ConsolidateBy(ConsolidateFunction.sum);
            Assert.Equal("maxSeries(consolidateBy(metric,'max'),consolidateBy(metric,'sum'))", new SeriesListBase[] { serie1, serie2 }.MaxSeries().ToString());

            IEnumerable<SeriesListBase> sources = new[] { _series };
            Assert.Equal("maxSeries(metric)", sources.MaxSeries().ToString());
        }

        [Fact]
        public void MaximumAbove()
        {
            var series = _series.MaximumAbove(10);
            Assert.Equal("maximumAbove(metric,10)", series.ToString());
            series = _series.MaximumAbove(1.1);
            Assert.Equal("maximumAbove(metric,1.1)", series.ToString());
        }

        [Fact]
        public void MaximumBelow()
        {
            var series = _series.MaximumBelow(10);
            Assert.Equal("maximumBelow(metric,10)", series.ToString());
            series = _series.MaximumBelow(1.1);
            Assert.Equal("maximumBelow(metric,1.1)", series.ToString());
        }

        [Fact]
        public void MinSeries()
        {
            var series = _series.MinSeries();
            Assert.Equal("minSeries(metric)", series.ToString());

            var sumSeries = _series.MinSeries(new GraphitePath("test1"), new GraphitePath("test2"));
            Assert.Equal("minSeries(metric,test1,test2)", sumSeries.ToString());

            var serie1 = _series.ConsolidateBy(ConsolidateFunction.max);
            var serie2 = _series.ConsolidateBy(ConsolidateFunction.sum);
            Assert.Equal("minSeries(consolidateBy(metric,'max'),consolidateBy(metric,'sum'))", new SeriesListBase[] { serie1, serie2 }.MinSeries().ToString());

            IEnumerable<SeriesListBase> sources = new[] { _series };
            Assert.Equal("minSeries(metric)", sources.MinSeries().ToString());
        }

        [Fact]
        public void MinimumAbove()
        {
            var series = _series.MinimumAbove(10);
            Assert.Equal("minimumAbove(metric,10)", series.ToString());
            series = _series.MinimumAbove(1.1);
            Assert.Equal("minimumAbove(metric,1.1)", series.ToString());
        }

        [Fact]
        public void MinimumBelow()
        {
            var series = _series.MinimumBelow(10);
            Assert.Equal("minimumBelow(metric,10)", series.ToString());
            series = _series.MinimumBelow(1.1);
            Assert.Equal("minimumBelow(metric,1.1)", series.ToString());
        }

        [Fact]
        public void MostDeviant()
        {
            var series = _series.MostDeviant(10);
            Assert.Equal("mostDeviant(metric,10)", series.ToString());
        }

        [Fact]
        public void MovingAverage()
        {
            var series = _series.MovingAverage(10);
            Assert.Equal("movingAverage(metric,10)", series.ToString());
            series = _series.MovingAverage("5min");
            Assert.Equal("movingAverage(metric,'5min')", series.ToString());
        }

        [Fact]
        public void MovingMedian()
        {
            var series = _series.MovingMedian(10);
            Assert.Equal("movingMedian(metric,10)", series.ToString());
            series = _series.MovingMedian("5min");
            Assert.Equal("movingMedian(metric,'5min')", series.ToString());
        }

        [Fact]
        public void MultiplySeries()
        {
            var series = _series.MultiplySeries();
            Assert.Equal("multiplySeries(metric)", series.ToString());
        }

        [Fact]
        public void MultiplySeriesWithWildcards()
        {
            var series = _series.MultiplySeriesWithWildcards(1, 5);
            Assert.Equal("multiplySeriesWithWildcards(metric,1,5)", series.ToString());
        }

        [Fact]
        public void NPercentile()
        {
            var series = _series.NPercentile(10);
            Assert.Equal("nPercentile(metric,10)", series.ToString());
        }

        [Fact]
        public void NonNegativeDerivative()
        {
            var series = _series.NonNegativeDerivative();
            Assert.Equal("nonNegativeDerivative(metric)", series.ToString());
        }

        [Fact]
        public void Offset()
        {
            var series = _series.Offset(10);
            Assert.Equal("offset(metric,10)", series.ToString());
        }

        [Fact]
        public void OffsetToZero()
        {
            var series = _series.OffsetToZero();
            Assert.Equal("offsetToZero(metric)", series.ToString());
        }

        [Fact]
        public void PerSecond()
        {
            var series = _series.PerSecond();
            Assert.Equal("perSecond(metric)", series.ToString());
        }

        [Fact]
        public void PercentileOfSeries()
        {
            var series = _series.PercentileOfSeries(10, false);
            Assert.Equal("percentileOfSeries(metric,10)", series.ToString());
            series = _series.PercentileOfSeries(10, true);
            Assert.Equal("percentileOfSeries(metric,10,1)", series.ToString());
        }

        [Fact]
        public void Pow()
        {
            var series = _series.Pow(10);
            Assert.Equal("pow(metric,10)", series.ToString());
        }

        [Fact]
        public void RangeOfSeries()
        {
            var series = _series.RangeOfSeries();
            Assert.Equal("rangeOfSeries(metric)", series.ToString());
        }

        [Fact]
        public void RemoveAbovePercentile()
        {
            var series = _series.RemoveAbovePercentile(10);
            Assert.Equal("removeAbovePercentile(metric,10)", series.ToString());
        }

        [Fact]
        public void RemoveAboveValue()
        {
            var series = _series.RemoveAboveValue(10);
            Assert.Equal("removeAboveValue(metric,10)", series.ToString());
            series = _series.RemoveAboveValue(1.1);
            Assert.Equal("removeAboveValue(metric,1.1)", series.ToString());
        }

        [Fact]
        public void RemoveBelowPercentile()
        {
            var series = _series.RemoveBelowPercentile(10);
            Assert.Equal("removeBelowPercentile(metric,10)", series.ToString());
        }

        [Fact]
        public void RemoveBelowValue()
        {
            var series = _series.RemoveBelowValue(10);
            Assert.Equal("removeBelowValue(metric,10)", series.ToString());
            series = _series.RemoveBelowValue(1.1);
            Assert.Equal("removeBelowValue(metric,1.1)", series.ToString());
        }

        [Fact]
        public void RemoveBetweenPercentile()
        {
            var series = _series.RemoveBetweenPercentile(10);
            Assert.Equal("removeBetweenPercentile(metric,10)", series.ToString());
        }

        [Fact]
        public void RemoveEmptySeries()
        {
            var series = _series.RemoveEmptySeries();
            Assert.Equal("removeEmptySeries(metric)", series.ToString());
        }

        [Fact]
        public void Scale()
        {
            var series = _series.Scale(10);
            Assert.Equal("scale(metric,10)", series.ToString());
            series = _series.Scale(1000.1);
            Assert.Equal("scale(metric,1000.1)", series.ToString());
            series = _series.Scale(0.0005);
            Assert.Equal("scale(metric,0.0005)", series.ToString());
        }

        [Fact]
        public void ScaleToSeconds()
        {
            var series = _series.ScaleToSeconds(10);
            Assert.Equal("scaleToSeconds(metric,10)", series.ToString());
        }

        [Fact]
        public void SmartSummarize()
        {
            var series = _series.SmartSummarize("1day", x => x.Sum);
            Assert.Equal("smartSummarize(metric,'1day','sum')", series.ToString());
        }

        [Fact]
        public void SortByMaxima()
        {
            var series = _series.SortByMaxima();
            Assert.Equal("sortByMaxima(metric)", series.ToString());
        }

        [Fact]
        public void SortByMinima()
        {
            var series = _series.SortByMinima();
            Assert.Equal("sortByMinima(metric)", series.ToString());
        }

        [Fact]
        public void SortByName()
        {
            var series = _series.SortByName();
            Assert.Equal("sortByName(metric)", series.ToString());
            series = _series.SortByName(true);
            Assert.Equal("sortByName(metric,1)", series.ToString());
        }

        [Fact]
        public void SortByTotal()
        {
            var series = _series.SortByTotal();
            Assert.Equal("sortByTotal(metric)", series.ToString());
        }

        [Fact]
        public void SquareRoot()
        {
            var series = _series.SquareRoot();
            Assert.Equal("squareRoot(metric)", series.ToString());
        }

        [Fact]
        public void StddevSeries()
        {
            var series = _series.StddevSeries();
            Assert.Equal("stddevSeries(metric)", series.ToString());
        }

        [Fact]
        public void Stdev()
        {
            var series = _series.Stdev(1, 0.3);
            Assert.Equal("stdev(metric,1,0.3)", series.ToString());
            series = _series.Stdev(1);
            Assert.Equal("stdev(metric,1)", series.ToString());
        }

        [Fact]
        public void Substr()
        {
            var series = _series.Substr(2, 4);
            Assert.Equal("substr(metric,2,4)", series.ToString());
        }

        [Fact]
        public void Sum()
        {
            var series = _series.Sum();
            Assert.Equal("sum(metric)", series.ToString());

            var sumSeries = _series.Sum(new GraphitePath("test1"), new GraphitePath("test2"));
            Assert.Equal("sum(metric,test1,test2)", sumSeries.ToString());

            var serie1 = _series.ConsolidateBy(ConsolidateFunction.max);
            var serie2 = _series.ConsolidateBy(ConsolidateFunction.sum);
            Assert.Equal("sum(consolidateBy(metric,'max'),consolidateBy(metric,'sum'))", new SeriesListBase[] {serie1, serie2}.Sum().ToString());

            IEnumerable<SeriesListBase> sources = new[] { _series };
            Assert.Equal("sum(metric)", sources.Sum().ToString());
        }

        [Fact]
        public void SumSeriesWithWildcards()
        {
            var series = _series.SumSeriesWithWildcards(1, 4);
            Assert.Equal("sumSeriesWithWildcards(metric,1,4)", series.ToString());
        }

        [Fact]
        public void Summarize()
        {
            var series = _series.Summarize("1day", SummarizeFunction.sum);
            Assert.Equal("summarize(metric,'1day','sum')", series.ToString());
        }

        [Fact]
        public void TimeShift()
        {
            var series = _series.TimeShift("1day", resetEnd: true, alignDst: false);
            Assert.Equal("timeShift(metric,'1day')", series.ToString());
            series = _series.TimeShift("1day", resetEnd: false, alignDst: true);
            Assert.Equal("timeShift(metric,'1day',0,1)", series.ToString());
        }

        [Fact]
        public void TimeSlice()
        {
            var series = _series.TimeSlice("-7day", "-1day");
            Assert.Equal("timeSlice(metric,'-7day','-1day')", series.ToString());
            series = _series.TimeSlice("-7day");
            Assert.Equal("timeSlice(metric,'-7day')", series.ToString());
            series = _series.TimeSlice("-7day", "now");
            Assert.Equal("timeSlice(metric,'-7day')", series.ToString());
        }

        [Fact]
        public void TimeStack()
        {
            var series = _series.TimeStack("-7day", 0, 7);
            Assert.Equal("timeStack(metric,'-7day',0,7)", series.ToString());
        }

        [Fact]
        public void TransformNull()
        {
            var series = _series.TransformNull();
            Assert.Equal("transformNull(metric)", series.ToString());
            series = _series.TransformNull(0);
            Assert.Equal("transformNull(metric)", series.ToString());
            series = _series.TransformNull(10);
            Assert.Equal("transformNull(metric,10)", series.ToString());
            series = _series.TransformNull(10, _series);
            Assert.Equal("transformNull(metric,10,metric)", series.ToString());
        }

        [Fact]
        public void UseSeriesAbove()
        {
            var series = _series.UseSeriesAbove(10, "reqs", "time");
            Assert.Equal("useSeriesAbove(metric,10,\"reqs\",\"time\")", series.ToString());
            series = _series.UseSeriesAbove(1.1, "reqs", "time");
            Assert.Equal("useSeriesAbove(metric,1.1,\"reqs\",\"time\")", series.ToString());
        }

        [Fact]
        public void WeightedAverage()
        {
            var series = _series.WeightedAverage(_series, 1, 3, 4);
            Assert.Equal("weightedAverage(metric,metric,1,3,4)", series.ToString());
        }

        [Fact]
        public void TemplatePositional()
        {
            var series = _series.Template("worker1");
            Assert.Equal("template(metric,\"worker1\")", series.ToString());
        }

        [Fact]
        public void TemplateNamed()
        {
            var series = _series.Template(new Tuple<string, string>("hostname", "worker1"));
            Assert.Equal("template(metric,hostname=\"worker1\")", series.ToString());
        }
    }
}