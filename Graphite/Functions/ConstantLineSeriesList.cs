using ahd.Graphite.Base;

namespace ahd.Graphite.Functions
{
    /// <summary>
    /// Draws a horizontal line at value F across the graph.
    /// </summary>
    public class ConstantLineSeriesList : SeriesListBase
    {
        public ConstantLineSeriesList(double value)
        {
            Value = value;
        }

        public double Value { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"constantLine({Value})";
        }
    }
}