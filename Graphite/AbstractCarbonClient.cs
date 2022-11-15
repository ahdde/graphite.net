using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ahd.Graphite
{
    /// <summary>
    /// Base class for clients for submitting data to carbon
    /// </summary>
    public abstract class AbstractCarbonClient
    {
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
        /// Send a list of datapoints
        /// </summary>
        /// <param name="datapoints"></param>
        /// <returns></returns>
        public Task SendAsync(params Datapoint[] datapoints)
        {
            return SendAsync(datapoints, CancellationToken.None);
        }

        /// <summary>
        /// Send a list of datapoints
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <param name="datapoints"></param>
        /// <returns></returns>
        public Task SendAsync(CancellationToken cancellationToken, params Datapoint[] datapoints)
        {
            return SendAsync(datapoints, cancellationToken);
        }

        /// <summary>
        /// Send a list of datapoints
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
        /// Send a list of datapoints
        /// </summary>
        /// <param name="datapoints"></param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns></returns>
        public abstract Task SendAsync(ICollection<Datapoint> datapoints, CancellationToken cancellationToken = default(CancellationToken));
        
    }
}