using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ahd.Graphite
{
    /// <summary>
    /// graphite formatter for sending data to graphite in different formats
    /// </summary>
    public interface IGraphiteFormatter
    {
        /// <summary>
        /// Format the datapoints an write async to the target stream
        /// </summary>
        /// <param name="stream">targetstream to write the data to</param>
        /// <param name="datapoints">list of datapoints to send</param>
        Task WriteAsync(Stream stream, ICollection<Datapoint> datapoints);

        /// <summary>
        /// Format the datapoints an write to the target stream
        /// </summary>
        /// <param name="stream">targetstream to write the data to</param>
        /// <param name="datapoints">list of datapoints to send</param>
        void Write(Stream stream, ICollection<Datapoint> datapoints);

        /// <summary>
        /// target port to send the data to
        /// </summary>
        ushort Port { get; }
    }
}