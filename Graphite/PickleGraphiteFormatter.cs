using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Razorvine.Pickle;

namespace ahd.Graphite
{
    public class PickleGraphiteFormatter : IGraphiteFormatter
    {
        public ushort Port { get; }
        
        public PickleGraphiteFormatter(ushort port):this()
        {
            Port = port;
        }

        public PickleGraphiteFormatter()
        {
            Port = 2004;
        }

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
                await stream.WriteAsync(sizeBytes, 0, sizeBytes.Length);

                await stream.WriteAsync(pickled, 0, pickled.Length);
                await stream.FlushAsync();
            }
        }

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