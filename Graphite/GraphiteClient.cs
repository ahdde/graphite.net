using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ahd.Graphite.Base;
using ahd.Graphite.Exceptions;

namespace ahd.Graphite
{
    /// <summary>
    /// Client for submitting data to and querying from graphite
    /// </summary>
    public class GraphiteClient
    {
        /// <summary>
        /// Creates a client with the specified host and formatter
        /// </summary>
        /// <param name="host">Graphite hostname</param>
        /// <param name="formatter">formatter for sending data to graphite</param>
        public GraphiteClient(string host, IGraphiteFormatter formatter) : this(host)
        {
            if (formatter == null) throw new ArgumentNullException(nameof(formatter));

            Formatter = formatter;
        }
        
        /// <summary>
        /// Creates a client with the specified host
        /// </summary>
        /// <param name="host">Graphite hostname</param>
        public GraphiteClient(string host):this()
        {
            if (String.IsNullOrEmpty(host)) throw new ArgumentNullException(nameof(host));

            Host = host;
        }

        /// <summary>
        /// Creates a client for localhost
        /// </summary>
        public GraphiteClient()
        {
            Host = "localhost";
            UseSsl = true;
            HttpApiPort = 443;
            Formatter = new PlaintextGraphiteFormatter();
            BatchSize = 500;
        }
        
        /// <summary>
        /// graphite hostname - default "localhost"
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// Use ssl for query - default "true"
        /// </summary>
        public bool UseSsl { get; set; }

        /// <summary>
        /// port for query - default "443" 
        /// For data transmissions, the value specified in the <see cref="Formatter"/>'s <see cref="IGraphiteFormatter.Port"/> is used.
        /// </summary>
        public ushort HttpApiPort { get; set; }

        /// <summary>
        /// Formatter for sending data - default <see cref="PlaintextGraphiteFormatter"/>
        /// </summary>
        public IGraphiteFormatter Formatter { get; }

        /// <summary>
        /// Maximum number of datapoints to send in a single request - default "500"
        /// </summary>
        public int BatchSize { get; set; }

        /// <summary>
        /// Send a single datapoint
        /// </summary>
        /// <param name="series">metric path</param>
        /// <param name="value">metric value</param>
        /// <returns></returns>
        public Task SendAsync(string series, double value)
        {
            return SendAsync(series, value, DateTime.Now);
        }

        /// <summary>
        /// Send a single datapoint
        /// </summary>
        /// <param name="series">metric path</param>
        /// <param name="value">metric value</param>
        /// <param name="timestamp">metric timestamp</param>
        /// <returns></returns>
        public Task SendAsync(string series, double value, DateTime timestamp)
        {
            return SendAsync(new Datapoint(series, value, timestamp));
        }

        /// <summary>
        /// Send a list of datapoints in up to <see cref="BatchSize"/> batches
        /// </summary>
        /// <param name="datapoints"></param>
        /// <returns></returns>
        public Task SendAsync(params Datapoint[] datapoints)
        {
            ICollection<Datapoint> points = datapoints;
            return SendAsync(points);
        }

        /// <summary>
        /// Send a list of datapoints in up to <see cref="BatchSize"/> batches
        /// </summary>
        /// <param name="datapoints"></param>
        /// <returns></returns>
        public async Task SendAsync(ICollection<Datapoint> datapoints)
        {
            if (datapoints == null || datapoints.Count == 0) throw new ArgumentNullException(nameof(datapoints));
            var batches = GetBatches(datapoints);
            foreach (var batch in batches)
            {
                await SendInternalAsync(batch).ConfigureAwait(false);
            }
        }

        private async Task SendInternalAsync(ICollection<Datapoint> datapoints)
        {
            using (var client = new TcpClient(AddressFamily.InterNetworkV6))
            {
                client.Client.DualMode = true;
                await client.ConnectAsync(Host, Formatter.Port).ConfigureAwait(false);
                using (var stream = client.GetStream())
                {
                    await Formatter.WriteAsync(stream, datapoints).ConfigureAwait(false);
                }
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
            using (var client = new TcpClient(Host, Formatter.Port))
            {
                using (var stream = client.GetStream())
                {
                    Formatter.Write(stream, datapoints);
                }
            }
        }

        /// <summary>
        /// Walks the metrics tree and returns every metric found as a sorted JSON array
        /// </summary>
        /// <returns>list of all metrics</returns>
        public async Task<string[]> GetAllMetricsAsync()
        {
            using (var client = new HttpClient {BaseAddress = GetHttpApiUri()})
            {
                var response = await client.GetAsync("/metrics/index.json").ConfigureAwait(false);
                await response.EnsureSuccessStatusCodeAsync().ConfigureAwait(false);
                return await response.Content.ReadAsAsync<string[]>().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Finds metrics under a given path
        /// </summary>
        /// <param name="query">The query to search for</param>
        /// <param name="wildcards">Whether to add a wildcard result at the end or not</param>
        /// <param name="from">timestamp from which to consider metrics</param>
        /// <param name="until">timestamp until which to consider metrics</param>
        /// <returns></returns>
        public async Task<GraphiteMetric[]> FindMetricsAsync(string query, bool wildcards = false, DateTime? from = null, DateTime? until = null)
        {
            if (String.IsNullOrEmpty(query)) throw new ArgumentNullException(nameof(query));

            using (var client = new HttpClient {BaseAddress = GetHttpApiUri()})
            {
                query = WebUtility.UrlEncode(query);
                string fromUnix = String.Empty;
                string untilUnix = String.Empty;
                if (from.HasValue)
                    fromUnix = Datapoint.ToUnixTimestamp(from.Value).ToString();
                if (until.HasValue)
                    untilUnix = Datapoint.ToUnixTimestamp(until.Value).ToString();

                var uri = String.Format("/metrics/find?query={0}&format=completer&wildcards={1}&from={2}&until={3}",
                    query, wildcards ? 1 : 0, fromUnix, untilUnix);

                var response = await client.GetAsync(uri).ConfigureAwait(false);
                await response.EnsureSuccessStatusCodeAsync().ConfigureAwait(false);
                return (await response.Content.ReadAsAsync<GraphiteFindResult>().ConfigureAwait(false)).Metrics;
            }
        }

        /// <summary>
        /// Expands the given query with matching paths
        /// </summary>
        /// <param name="query">The metrics query</param>
        /// <returns>list of matching metric names</returns>
        public Task<string[]> ExpandMetricsAsync(params GraphitePath[] query)
        {
            return ExpandMetricsAsync(false, query);
        }

        /// <summary>
        /// Expands the given query with matching paths
        /// </summary>
        /// <param name="leavesOnly">Whether to only return leaves or both branches and leaves</param>
        /// <param name="query">The metrics query</param>
        /// <returns>list of matching metric names</returns>
        public async Task<string[]> ExpandMetricsAsync(bool leavesOnly, params GraphitePath[] query)
        {
            if (query == null || query.Length == 0) throw new ArgumentNullException(nameof(query));

            using (var client = new HttpClient {BaseAddress = GetHttpApiUri()})
            {
                var values = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("leavesOnly",leavesOnly ? "1" : "0"),
                };
                foreach (var q in query)
                {
                    values.Add(new KeyValuePair<string, string>("query", q.ToString()));
                }

                var body = new FormUrlEncodedContent(values);
                var response = await client.PostAsync("/metrics/expand", body).ConfigureAwait(false);
                await response.EnsureSuccessStatusCodeAsync().ConfigureAwait(false);
                return (await response.Content.ReadAsAsync<GraphiteExpandResult>().ConfigureAwait(false)).Results;
            }
        }

        private Uri GetHttpApiUri()
        {
            var builder = new UriBuilder("http", Host, HttpApiPort);
            if (UseSsl)
                builder.Scheme = "https";
            return builder.Uri;
        }

        /// <summary>
        /// fetch metric values from graphite
        /// </summary>
        /// <param name="target">path identifying one metric, optionally with functions acting on those metric</param>
        /// <param name="from">specify the relative or absolute beginning to graph</param>
        /// <param name="until">specify the relative or absolute end to graph</param>
        /// <param name="template">The target metrics can use a special <see cref="SeriesListBase.Template(string[])"/> function which allows the metric paths to contain variables</param>
        /// <param name="maxDataPoints"></param>
        /// <returns></returns>
        public Task<GraphiteMetricData[]> GetMetricsDataAsync(SeriesListBase target, string from = null, string until = null, IDictionary<string, string> template = null, ulong? maxDataPoints = null)
        {
            return GetMetricsDataAsync(new[] {target}, from, until, template, maxDataPoints);
        }

        /// <summary>
        /// fetch metric values from graphite
        /// </summary>
        /// <param name="targets">path identifying one or several metrics, optionally with functions acting on those metrics</param>
        /// <param name="from">specify the relative or absolute beginning to graph</param>
        /// <param name="until">specify the relative or absolute end to graph</param>
        /// <param name="template">The target metrics can use a special <see cref="SeriesListBase.Template(string[])"/> function which allows the metric paths to contain variables</param>
        /// <param name="maxDataPoints"></param>
        /// <returns></returns>
        public async Task<GraphiteMetricData[]> GetMetricsDataAsync(SeriesListBase[] targets, string from = null, string until = null, IDictionary<string,string> template = null, ulong? maxDataPoints = null)
        {
            if (targets == null || targets.Length == 0) throw new ArgumentNullException(nameof(targets));

            using (var client = new HttpClient {BaseAddress = GetHttpApiUri()})
            {
                var values = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("format","json"),
                };

                foreach (var target in targets)
                {
                    values.Add(new KeyValuePair<string, string>("target", target.ToString()));
                }

                if (!String.IsNullOrEmpty(from))
                {
                    values.Add(new KeyValuePair<string, string>("from", from));
                }
                if (!String.IsNullOrEmpty(until))
                {
                    values.Add(new KeyValuePair<string, string>("until", until));
                }
                if (template != null)
                {
                    foreach (var kvp in template)
                    {
                        values.Add(new KeyValuePair<string, string>($"template[{kvp.Key}]", kvp.Value));
                    }
                }
                if (maxDataPoints.HasValue)
                {
                    values.Add(new KeyValuePair<string, string>("maxDataPoints", maxDataPoints.Value.ToString()));
                }

                //workaround for size limit in FormUrlEncodedContent
                var sb = new StringBuilder();
                foreach (var value in values)
                {
                    if (sb.Length > 0)
                        sb.Append("&");
                    sb.Append(WebUtility.UrlEncode(value.Key)).Append("=").Append(WebUtility.UrlEncode(value.Value));
                }
                var body = new StringContent(sb.ToString(), Encoding.UTF8, "application/x-www-form-urlencoded");

                var response = await client.PostAsync("/render",body).ConfigureAwait(false);
                await response.EnsureSuccessStatusCodeAsync().ConfigureAwait(false);

                return await response.Content.ReadAsAsync<GraphiteMetricData[]>().ConfigureAwait(false);
            }
        }
    }
}
