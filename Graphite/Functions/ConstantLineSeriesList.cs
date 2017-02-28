using ahd.Graphite.Base;

namespace ahd.Graphite.Functions
{
    /// <summary>
    /// Draws a horizontal line at value F across the graph.
    /// </summary>
    public class ConstantLineSeriesList : SeriesListBase
    {
        /// <summary>
        /// Draws a horizontal line at value F across the graph.
        /// </summary>
        /// <param name="value">the value F</param>
        public ConstantLineSeriesList(double value)
        {
            Value = value;
        }

        /// <summary>
        /// value to draw the line at
        /// </summary>
        public double Value { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"constantLine({Value})";
        }
    }
}