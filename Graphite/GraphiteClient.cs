using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ahd.Graphite.Base;
using ahd.Graphite.Exceptions;

namespace ahd.Graphite
{
    /// <summary>
    /// Client for querying data from graphite
    /// </summary>
    public class GraphiteClient
    {
        private readonly HttpClient _client;
        
        /// <summary>
        /// Creates a client for localhost
        /// </summary>
        public GraphiteClient():this("https://localhost")
        {
        }
        /// <summary>
        /// Creates a client with the specified endpoint
        /// </summary>
        /// <param name="baseAddress">graphite api endpoint</param>
        public GraphiteClient(string baseAddress):this(new Uri(baseAddress))
        {
        }

        /// <summary>
        /// Creates a client with the specified endpoint
        /// </summary>
        /// <param name="baseAddress">graphite api endpoint</param>
        public GraphiteClient(Uri baseAddress):this(new HttpClient{BaseAddress = baseAddress})
        {
        }

        /// <summary>
        /// Creates a client using the supplied http client
        /// </summary>
        /// <param name="client">preconfigured http client</param>
        public GraphiteClient(HttpClient client)
        {
            _client = client;
        }
        
        /// <summary>
        /// Walks the metrics tree and returns every metric found as a sorted JSON array
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>list of all metrics</returns>
        public async Task<string[]> GetAllMetricsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = await _client.GetAsync("metrics/index.json", cancellationToken).ConfigureAwait(false);
            await response.EnsureSuccessStatusCodeAsync().ConfigureAwait(false);
            return await response.Content.ReadAsAsync<string[]>(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Finds metrics under a given path
        /// </summary>
        /// <param name="query">The query to search for</param>
        /// <param name="wildcards">Whether to add a wildcard result at the end or not</param>
        /// <param name="from">timestamp from which to consider metrics</param>
        /// <param name="until">timestamp until which to consider metrics</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns></returns>
        public async Task<GraphiteMetric[]> FindMetricsAsync(string query, bool wildcards = false, DateTime? from = null, DateTime? until = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (String.IsNullOrEmpty(query)) throw new ArgumentNullException(nameof(query));

            query = WebUtility.UrlEncode(query);
            string fromUnix = String.Empty;
            string untilUnix = String.Empty;
            if (from.HasValue)
                fromUnix = Datapoint.ToUnixTimestamp(from.Value).ToString();
            if (until.HasValue)
                untilUnix = Datapoint.ToUnixTimestamp(until.Value).ToString();

            var uri = String.Format("metrics/find?query={0}&format=completer&wildcards={1}&from={2}&until={3}",
                query, wildcards ? 1 : 0, fromUnix, untilUnix);

            var response = await _client.GetAsync(uri, cancellationToken).ConfigureAwait(false);
            await response.EnsureSuccessStatusCodeAsync().ConfigureAwait(false);
            return (await response.Content.ReadAsAsync<GraphiteFindResult>(cancellationToken).ConfigureAwait(false))
                .Metrics;
        }

        /// <summary>
        /// Expands the given query with matching paths
        /// </summary>
        /// <param name="query">The metrics query</param>
        /// <returns>list of matching metric names</returns>
        public Task<string[]> ExpandMetricsAsync(params GraphitePath[] query)
        {
            return ExpandMetricsAsync(false, CancellationToken.None, query);
        }

        /// <summary>
        /// Expands the given query with matching paths
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <param name="query">The metrics query</param>
        /// <returns>list of matching metric names</returns>
        public Task<string[]> ExpandMetricsAsync(CancellationToken cancellationToken, params GraphitePath[] query)
        {
            return ExpandMetricsAsync(false, cancellationToken, query);
        }

        /// <summary>
        /// Expands the given query with matching paths
        /// </summary>
        /// <param name="leavesOnly">Whether to only return leaves or both branches and leaves</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <param name="query">The metrics query</param>
        /// <returns>list of matching metric names</returns>
        public async Task<string[]> ExpandMetricsAsync(bool leavesOnly, CancellationToken cancellationToken, params GraphitePath[] query)
        {
            if (query == null || query.Length == 0) throw new ArgumentNullException(nameof(query));

            var values = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("leavesOnly", leavesOnly ? "1" : "0"),
            };
            foreach (var q in query)
            {
                values.Add(new KeyValuePair<string, string>("query", q.ToString()));
            }

            var body = new FormUrlEncodedContent(values);
            var response = await _client.PostAsync("metrics/expand", body, cancellationToken).ConfigureAwait(false);
            await response.EnsureSuccessStatusCodeAsync().ConfigureAwait(false);
            return (await response.Content.ReadAsAsync<GraphiteExpandResult>(cancellationToken).ConfigureAwait(false))
                .Results;
        }

        /// <summary>
        /// fetch metric values from graphite
        /// </summary>
        /// <param name="target">path identifying one metric, optionally with functions acting on those metric</param>
        /// <param name="from">specify the relative or absolute beginning to graph</param>
        /// <param name="until">specify the relative or absolute end to graph</param>
        /// <param name="template">The target metrics can use a special <see cref="SeriesListBase.Template(string[])"/> function which allows the metric paths to contain variables</param>
        /// <param name="maxDataPoints"></param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <param name="tz">specify time zone for from/until. Corresponds to 'tz' parameter in Graphite Render API.</param>
        /// <returns></returns>
        public Task<GraphiteMetricData[]> GetMetricsDataAsync(SeriesListBase target, string from = null, string until = null, IDictionary<string, string> template = null, ulong? maxDataPoints = null, string tz = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return GetMetricsDataAsync(new[] {target}, from, until, template, maxDataPoints, tz, cancellationToken);
        }

        /// <summary>
        /// fetch metric values from graphite
        /// </summary>
        /// <param name="targets">path identifying one or several metrics, optionally with functions acting on those metrics</param>
        /// <param name="from">specify the relative or absolute beginning to graph</param>
        /// <param name="until">specify the relative or absolute end to graph</param>
        /// <param name="template">The target metrics can use a special <see cref="SeriesListBase.Template(string[])"/> function which allows the metric paths to contain variables</param>
        /// <param name="maxDataPoints"></param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <param name="tz">specify time zone for from/until. Corresponds to 'tz' parameter in Graphite Render API.</param>
        /// <returns></returns>
        public async Task<GraphiteMetricData[]> GetMetricsDataAsync(SeriesListBase[] targets, string from = null, string until = null, IDictionary<string,string> template = null, ulong? maxDataPoints = null, string tz = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (targets == null || targets.Length == 0) throw new ArgumentNullException(nameof(targets));

            var values = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("format", "json"),
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

            if (!String.IsNullOrEmpty(tz))
            {
                values.Add(new KeyValuePair<string, string>("tz", tz));
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

            var response = await _client.PostAsync("render", body, cancellationToken).ConfigureAwait(false);
            await response.EnsureSuccessStatusCodeAsync().ConfigureAwait(false);

            return await response.Content.ReadAsAsync<GraphiteMetricData[]>(cancellationToken).ConfigureAwait(false);
        }
    }
}
