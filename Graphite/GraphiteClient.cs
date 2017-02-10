using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using ahd.Graphite.Base;

namespace ahd.Graphite
{
    public class GraphiteClient
    {
        public GraphiteClient(string host, IGraphiteFormatter formatter) : this(host)
        {
            if (formatter == null) throw new ArgumentNullException(nameof(formatter));

            Formatter = formatter;
        }
        
        public GraphiteClient(string host):this()
        {
            if (String.IsNullOrEmpty(host)) throw new ArgumentNullException(nameof(host));

            Host = host;
        }

        public GraphiteClient()
        {
            Host = "localhost";
            UseSsl = true;
            HttpApiPort = 443;
            Formatter = new PlaintextGraphiteFormatter();
            BatchSize = 500;
        }
        
        public string Host { get; }

        public bool UseSsl { get; set; }

        public ushort HttpApiPort { get; set; }

        public IGraphiteFormatter Formatter { get; }

        public int BatchSize { get; set; }

        public async Task SendAsync(string series, double value)
        {
            await SendAsync(series, value, DateTime.Now);
        }

        public async Task SendAsync(string series, double value, DateTime timestamp)
        {
            await SendAsync(new Datapoint(series, value, timestamp));
        }

        public async Task SendAsync(params Datapoint[] datapoints)
        {
            ICollection<Datapoint> points = datapoints;
            await SendAsync(points);
        }

        public async Task SendAsync(ICollection<Datapoint> datapoints)
        {
            if (datapoints == null || datapoints.Count == 0) throw new ArgumentNullException(nameof(datapoints));
            var batches = GetBatches(datapoints);
            foreach (var batch in batches)
            {
                await SendInternalAsync(batch);
            }
        }

        private async Task SendInternalAsync(ICollection<Datapoint> datapoints)
        {
            using (var client = new TcpClient())
            {
                await client.ConnectAsync(Host, Formatter.Port);
                using (var stream = client.GetStream())
                {
                    await Formatter.WriteAsync(stream, datapoints);
                }
            }
        }

        public void Send(params Datapoint[] datapoints)
        {
            ICollection<Datapoint> points = datapoints;
            Send(points);
        }

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
            using (var client = new TcpClient())
            {
                client.Connect(Host, Formatter.Port);
                using (var stream = client.GetStream())
                {
                    Formatter.Write(stream, datapoints);
                }
            }
        }

        public async Task<string[]> GetAllMetricsAsync()
        {
            using (var client = new HttpClient {BaseAddress = GetHttpApiUri()})
            {
                var response = await client.GetAsync("/metrics/index.json");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsAsync<string[]>();
            }
        }

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

                var response = await client.GetAsync(uri);
                response.EnsureSuccessStatusCode();
                return (await response.Content.ReadAsAsync<GraphiteFindResult>()).Metrics;
            }
        }
        
        public Task<string[]> ExpandMetricsAsync(params GraphitePath[] query)
        {
            return ExpandMetricsAsync(false, query);
        }

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
                var response = await client.PostAsync("/metrics/expand", body);
                response.EnsureSuccessStatusCode();
                return (await response.Content.ReadAsAsync<GraphiteExpandResult>()).Results;
            }
        }

        private Uri GetHttpApiUri()
        {
            var builder = new UriBuilder("http", Host, HttpApiPort);
            if (UseSsl)
                builder.Scheme = "https";
            return builder.Uri;
        }

        public Task<GraphiteMetricData[]> GetMetricsDataAsync(SeriesListBase target, string from = null, string until = null, IDictionary<string, string> template = null, ulong? maxDataPoints = null)
        {
            return GetMetricsDataAsync(new[] {target}, from, until);
        }

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
                var body = new FormUrlEncodedContent(values);
                var response = await client.PostAsync("/render",body);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsAsync<GraphiteMetricData[]>();
            }
        }
    }
}
