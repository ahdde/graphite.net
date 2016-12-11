using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ahd.Graphite
{
    public interface IGraphiteFormatter
    {
        Task WriteAsync(Stream stream, ICollection<Datapoint> datapoints);

        void Write(Stream stream, ICollection<Datapoint> datapoints);

        ushort Port { get; }
    }
}