using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Razorvine.Pickle;

namespace ahd.Graphite
{
    /// <summary>
    /// formatter for sending data in python pickle format
    /// </summary>
    public class PickleGraphiteFormatter : IGraphiteFormatter
    {
        /// <inheritdoc/>
        public ushort Port { get; }
        
        /// <summary>
        /// Creates a pickle formatter with a custom destination port
        /// </summary>
        /// <param name="port">target port (default 2004)</param>
        public PickleGraphiteFormatter(ushort port):this()
        {
            Port = port;
        }

        /// <summary>
        /// Creates a pickle formatter with default port 2004
        /// </summary>
        public PickleGraphiteFormatter()
        {
            Port = 2004;
        }

        /// <inheritdoc/>
        public async Task WriteAsync(Stream stream, ICollection<Datapoint> datapoints)
        {
            using (var pickler = new Pickler())
            {
                var data = datapoints.Select(x => new object[] {x.Series, new object[] {x.UnixTimestamp, x.Value}});
                var pickled = pickler.dumps(data);

                //send header: struct.pack("!L", len(payload))
                uint size = (uint) pickled.LongLength;
                var sizeBytes = BitConverter.GetBytes(size);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(sizeBytes);
                await stream.WriteAsync(sizeBytes, 0, sizeBytes.Length).ConfigureAwait(false);

                await stream.WriteAsync(pickled, 0, pickled.Length).ConfigureAwait(false);
                await stream.FlushAsync().ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public void Write(Stream stream, ICollection<Datapoint> datapoints)
        {
            using (var pickler = new Pickler())
            {
                var data = datapoints.Select(x => new object[] { x.Series, new object[] { x.UnixTimestamp, x.Value } });
                var pickled = pickler.dumps(data);

                //send header: struct.pack("!L", len(payload))
                uint size = (uint) pickled.LongLength;
                var sizeBytes = BitConverter.GetBytes(size);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(sizeBytes);
                stream.Write(sizeBytes, 0, sizeBytes.Length);

                stream.Write(pickled, 0, pickled.Length);
                stream.Flush();
            }
        }
    }
}