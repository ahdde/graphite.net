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
        /// <summary>
        /// Creates a threshold series at value F
        /// </summary>
        /// <param name="value">value F</param>
        /// <param name="label">alias to use</param>
        public ThresholdSeriesList(double value, string label="")
        {
            if (label == null) throw new ArgumentNullException(nameof(label));
            Value = value;
            Label = label;
        }

        /// <summary>
        /// value F
        /// </summary>
        public double Value { get; }

        /// <summary>
        /// alias name of the series
        /// </summary>
        public string Label { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"threshold({Value},\"{Label}\")";
        }
    }
}