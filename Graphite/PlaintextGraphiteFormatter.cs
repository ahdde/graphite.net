using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace ahd.Graphite
{
    public class PlaintextGraphiteFormatter: IGraphiteFormatter
    {
        public PlaintextGraphiteFormatter()
        {
            Port = 2003;
        }

        public PlaintextGraphiteFormatter(ushort port):this()
        {
            Port = port;
        }

        public ushort Port { get; }

        public async Task WriteAsync(Stream stream, ICollection<Datapoint> datapoints)
        {
            using (var writer = new StreamWriter(stream) {NewLine = "\n"})
            {
                foreach (var datapoint in datapoints)
                {
                    await writer.WriteLineAsync($"{datapoint.Series} {datapoint.Value.ToString(CultureInfo.InvariantCulture)} {datapoint.UnixTimestamp}");
                }
                await writer.FlushAsync();
            }
        }

        public void Write(Stream stream, ICollection<Datapoint> datapoints)
        {
            using (var writer = new StreamWriter(stream) { NewLine = "\n" })
            {
                foreach (var datapoint in datapoints)
                {
                    writer.WriteLine($"{datapoint.Series} {datapoint.Value} {datapoint.UnixTimestamp}");
                }
                writer.Flush();
            }
        }
    }
}