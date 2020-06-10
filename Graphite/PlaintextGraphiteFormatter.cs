using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ahd.Graphite
{
    /// <summary>
    /// formatter for sending plaintext data
    /// </summary>
    public class PlaintextGraphiteFormatter: IGraphiteFormatter
    {
        private static readonly Encoding _utfNoBom = new UTF8Encoding(false, true);

        /// <summary>
        /// single space, will be trimmed by carbon
        /// </summary>
        private static readonly byte[] _empty = {32};

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
            using (var writer = new StreamWriter(stream, _utfNoBom, 1024, true) {NewLine = "\n"})
            {
                foreach (var datapoint in datapoints)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await writer.WriteLineAsync($"{datapoint.Series} {datapoint.Value.ToString(CultureInfo.InvariantCulture)} {datapoint.UnixTimestamp}").ConfigureAwait(false);
                }
                cancellationToken.ThrowIfCancellationRequested();
                await writer.FlushAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public void Write(Stream stream, ICollection<Datapoint> datapoints)
        {
            using (var writer = new StreamWriter(stream, _utfNoBom, 1024, true) { NewLine = "\n" })
            {
                foreach (var datapoint in datapoints)
                {
                    writer.WriteLine($"{datapoint.Series} {datapoint.Value.ToString(CultureInfo.InvariantCulture)} {datapoint.UnixTimestamp}");
                }
                writer.Flush();
            }
        }

        /// <inheritdoc/>
        public void TestConnection(Stream stream)
        {
            stream.Write(_empty, 0, _empty.Length);
            stream.Write(_empty, 0, _empty.Length);
            stream.Flush();
        }

        /// <inheritdoc/>
        public async Task TestConnectionAsync(Stream stream, CancellationToken cancellationToken)
        {
            await stream.WriteAsync(_empty, 0, _empty.Length, cancellationToken).ConfigureAwait(false);
            await stream.WriteAsync(_empty, 0, _empty.Length, cancellationToken).ConfigureAwait(false);
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}