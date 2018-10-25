using ahd.Graphite;

namespace Graphite.Core.Tests
{
    public class TestGraphiteFormatter : PlaintextGraphiteFormatter
    {
        public TestGraphiteFormatter(ushort port) : base(port)
        {
        }

        public TestGraphiteFormatter() : base()
        {
        }
    }
}