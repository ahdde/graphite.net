using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ahd.Graphite
{
    /// <summary>
    /// Client for submitting data to carbon
    /// </summary>
    public class CarbonClient
    {
        private readonly CarbonConnectionPool _carbonPool;
        
        /// <summary>
        /// Creates a client for localhost
        /// </summary>
        public CarbonClient():this("localhost")
        {
        }

        /// <summary>
        /// Creates a client with the specified host
        /// </summary>
        /// <param name="host">Graphite hostname</param>
        public CarbonClient(string host):this(host, new PlaintextGraphiteFormatter())
        {
        }
        /// <summary>
        /// Creates a client with the specified host and formatter
        /// </summary>
        /// <param name="host">carbon hostname</param>
        /// <param name="formatter">formatter for sending data to graphite</param>
        public CarbonClient(string host, IGraphiteFormatter formatter)
        {
            if (String.IsNullOrEmpty(host)) throw new ArgumentNullException(nameof(host));
            if (formatter == null) throw new ArgumentNullException(nameof(formatter));

            Formatter = formatter;
            Host = host;
            BatchSize = 500;
            UseDualStack = true;
            _carbonPool = new CarbonConnectionPool(Host, Formatter);
        }
        
        /// <summary>
        /// carbon hostname - default "localhost"
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// Formatter for sending data - default <see cref="PlaintextGraphiteFormatter"/>
        /// </summary>
        public IGraphiteFormatter Formatter { get; }
        
        /// <summary>
        /// Maximum number of datapoints to send in a single request - default "500"
        /// </summary>
        public int BatchSize { get; set; }
        
        /// <summary>
        /// Use ip dual stack for sending metrics. Defaults to true.
        /// </summary>
        public bool UseDualStack { get; set; }
        
        /// <summary>
        /// Send a single datapoint
        /// </summary>
        /// <param name="series">metric path</param>
        /// <param name="value">metric value</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns></returns>
        public Task SendAsync(string series, double value, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SendAsync(series, value, DateTime.Now, cancellationToken);
        }

        /// <summary>
        /// Send a single datapoint
        /// </summary>
        /// <param name="series">metric path</param>
        /// <param name="value">metric value</param>
        /// <param name="timestamp">metric timestamp</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns></returns>
        public Task SendAsync(string series, double value, DateTime timestamp, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SendAsync(new []{new Datapoint(series, value, timestamp)}, cancellationToken);
        }

        /// <summary>
        /// Send a list of datapoints in up to <see cref="BatchSize"/> batches
        /// </summary>
        /// <param name="datapoints"></param>
        /// <returns></returns>
        public Task SendAsync(params Datapoint[] datapoints)
        {
            return SendAsync(datapoints, CancellationToken.None);
        }

        /// <summary>
        /// Send a list of datapoints in up to <see cref="BatchSize"/> batches
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <param name="datapoints"></param>
        /// <returns></returns>
        public Task SendAsync(CancellationToken cancellationToken, params Datapoint[] datapoints)
        {
            return SendAsync(datapoints, cancellationToken);
        }

        /// <summary>
        /// Send a list of datapoints in up to <see cref="BatchSize"/> batches
        /// </summary>
        /// <param name="datapoints"></param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns></returns>
        public Task SendAsync(Datapoint[] datapoints, CancellationToken cancellationToken)
        {
            ICollection<Datapoint> points = datapoints;
            return SendAsync(points, cancellationToken);
        }

        /// <summary>
        /// Send a list of datapoints in up to <see cref="BatchSize"/> batches
        /// </summary>
        /// <param name="datapoints"></param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns></returns>
        public async Task SendAsync(ICollection<Datapoint> datapoints, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (datapoints == null || datapoints.Count == 0) throw new ArgumentNullException(nameof(datapoints));
            var batches = GetBatches(datapoints);
            foreach (var batch in batches)
            {
                await SendInternalAsync(batch, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task SendInternalAsync(ICollection<Datapoint> datapoints, CancellationToken cancellationToken)
        {
            TcpClient client = await _carbonPool.GetAsync(UseDualStack, cancellationToken).ConfigureAwait(false);
            try
            {
                var stream = client.GetStream();
                await Formatter.WriteAsync(stream, datapoints, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _carbonPool.Return(client);
            }
        }

        /// <summary>
        /// Send a single datapoint
        /// </summary>
        /// <param name="series">metric path</param>
        /// <param name="value">metric value</param>
        /// <returns></returns>
        public void Send(string series, double value)
        {
            Send(series, value, DateTime.Now);
        }

        /// <summary>
        /// Send a single datapoint
        /// </summary>
        /// <param name="series">metric path</param>
        /// <param name="value">metric value</param>
        /// <param name="timestamp">metric timestamp</param>
        /// <returns></returns>
        public void Send(string series, double value, DateTime timestamp)
        {
            Send(new Datapoint(series, value, timestamp));
        }
        /// <summary>
        /// Send a list of datapoints in up to <see cref="BatchSize"/> batches
        /// </summary>
        /// <param name="datapoints"></param>
        /// <returns></returns>
        public void Send(params Datapoint[] datapoints)
        {
            ICollection<Datapoint> points = datapoints;
            Send(points);
        }

        /// <summary>
        /// Send a list of datapoints in up to <see cref="BatchSize"/> batches
        /// </summary>
        /// <param name="datapoints"></param>
        /// <returns></returns>
        public void Send(ICollection<Datapoint> datapoints)
        {
            if (datapoints == null || datapoints.Count == 0) throw new ArgumentNullException(nameof(datapoints));
            var batches = GetBatches(datapoints);
            foreach (var batch in batches)
            {
                SendInternal(batch);
            }
        }

        private IEnumerable<ICollection<Datapoint>> GetBatches(ICollection<Datapoint> datapoints)
        {
            if (datapoints.Count <= BatchSize)
            {
                yield return datapoints;
                yield break;
            }
            var section = new List<Datapoint>(BatchSize);

            foreach (var item in datapoints)
            {
                section.Add(item);

                if (section.Count == BatchSize)
                {
                    yield return section.AsReadOnly();
                    section = new List<Datapoint>(BatchSize);
                }
            }

            if (section.Count > 0)
                yield return section.AsReadOnly();
        }

        private void SendInternal(ICollection<Datapoint> datapoints)
        {
            var client = _carbonPool.Get();
            try
            {
                var stream = client.GetStream();
                Formatter.Write(stream, datapoints);
            }
            finally
            {
                _carbonPool.Return(client);
            }
        }
    }
}