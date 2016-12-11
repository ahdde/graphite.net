using System;
using ahd.Graphite.Base;

namespace ahd.Graphite.Functions
{
    /// <summary>
    /// Takes a float F, followed by a label.<para/>
    /// Draws a horizontal line at value F across the graph.
    /// </summary>
    public class ThresholdSeriesList : SeriesListBase
    {
        public ThresholdSeriesList(double value, string label="")
        {
            if (label == null) throw new ArgumentNullException(nameof(label));
            Value = value;
            Label = label;
        }

        public double Value { get; }

        public string Label { get; }

        public override string ToString()
        {
            return $"threshold({Value},\"{Label}\")";
        }
    }
}