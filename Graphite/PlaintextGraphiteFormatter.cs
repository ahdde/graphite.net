using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ahd.Graphite
{
    /// <summary>
    /// formatter for sending plaintext data
    /// </summary>
    public class PlaintextGraphiteFormatter: IGraphiteFormatter
    {
        /// <summary>
        /// Creates a plaintext formatter with default port 2003
        /// </summary>
        public PlaintextGraphiteFormatter()
        {
            Port = 2003;
        }

        /// <summary>
        /// creates a plaintext formatter with custom destination port
        /// </summary>
        /// <param name="port"></param>
        public PlaintextGraphiteFormatter(ushort port):this()
        {
            Port = port;
        }

        /// <inheritdoc/>
        public ushort Port { get; }

        /// <inheritdoc/>
        public async Task WriteAsync(Stream stream, ICollection<Datapoint> datapoints, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var writer = new StreamWriter(stream) {NewLine = "\n"})
            {
                foreach (var datapoint in datapoints)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await writer.WriteLineAsync($"{datapoint.Series} {datapoint.Value.ToString(CultureInfo.InvariantCulture)} {datapoint.UnixTimestamp}");
                }
                cancellationToken.ThrowIfCancellationRequested();
                await writer.FlushAsync();
            }
        }

        /// <inheritdoc/>
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