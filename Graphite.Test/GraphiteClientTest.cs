using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ahd.Graphite.Base;
using Newtonsoft.Json;
using Razorvine.Pickle;
using Xunit;

namespace ahd.Graphite.Test
{
    public class GraphiteClientTest
    {
        private static readonly string GraphiteHost = "example.com";

        [Fact]
        public void CanCreateClient()
        {
            var client = new GraphiteClient();
        }

        [Fact]
        public void CanPickle()
        {
            var pickler = new Pickler();
            var data = new ArrayList {new object[] {"asdf", new object[] {123, 321}}};
            var pickled = pickler.dumps(data);
            foreach (var b in pickled)
            {
                if (b < 32 || b > 126)
                {
                    Console.Write("\\x");
                    Console.Write(BitConverter.ToString(new[] {b}));
                }
                else
                {
                    Console.Write(Encoding.ASCII.GetString(new [] {b}));
                }
            }
        }

        [Fact]
        public void ClientHasSaneDefaultValues()
        {
            var client = new GraphiteClient();
            Assert.Equal(2003, client.Formatter.Port);
            Assert.IsType<PlaintextGraphiteFormatter>(client.Formatter);
            Assert.Equal("localhost", client.Host);
        }

        [Fact]
        public void CanCreateClientWithParams()
        {
            var client = new GraphiteClient(GraphiteHost);
            Assert.Equal(GraphiteHost, client.Host);

            client = new GraphiteClient(GraphiteHost, new TestGraphiteFormatter(2004));
            Assert.Equal(GraphiteHost, client.Host);
            Assert.Equal(2004, client.Formatter.Port);
            Assert.IsType<TestGraphiteFormatter>(client.Formatter);
        }

        [Fact]
        public async Task CanSendMetric()
        {
            var server = new TcpListener(new IPEndPoint(IPAddress.Loopback, 33225));
            server.Start();
            var client = new GraphiteClient("localhost", new PlaintextGraphiteFormatter(33225));
            var recvTask = ReceiveMetric(server);
            var sendTask = client.SendAsync("usage.unittest.cpu.count", Environment.ProcessorCount);
            await sendTask;
            var metric = await recvTask;
            Assert.Contains("usage.unittest.cpu.count", metric);
            server.Stop();
            Console.WriteLine(metric);
        }

        [Fact]
        public void DataPointConversion()
        {
            var data = new Datapoint("test", 0, 1451338593);
            Assert.Equal(1451338593,data.UnixTimestamp);
        }

        private async Task<string> ReceiveMetric(TcpListener server)
        {
            using (var conn = await server.AcceptTcpClientAsync())
            using (var stream = conn.GetStream())
            using (var reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
            
        }
        
        [Fact]
        [Trait("Category", "Integration")]
        public async Task SendMetric()
        {
            var client = new GraphiteClient(GraphiteHost);
            await client.SendAsync("usage.unittest.cpu.count", Environment.ProcessorCount);
            Console.WriteLine("done");
        }
        
        [Fact]
        [Trait("Category", "Integration")]
        public async Task SendPickledMetric()
        {
            var client = new GraphiteClient(GraphiteHost, new PickleGraphiteFormatter());
            await client.SendAsync("usage.unittest.pickled.cpu.count", Environment.ProcessorCount);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task CanSendToV6OnlyHost()
        {
            var client = new GraphiteClient("ipv6.test-ipv6.com", new PlaintextGraphiteFormatter(80));
            await client.SendAsync("usage.unittest.cpu.count", 1);
            client.Send("usage.unittest.cpu.count", 1);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task CanSendToV4OnlyHost()
        {
            var client = new GraphiteClient("test-ipv6.com", new PlaintextGraphiteFormatter(80));
            await client.SendAsync("usage.unittest.cpu.count", 1);
            client.Send("usage.unittest.cpu.count", 1);
        }

        class TestGraphiteFormatter : PlaintextGraphiteFormatter
        {
            public TestGraphiteFormatter(ushort port):base(port)
            {
                
            }

            public TestGraphiteFormatter():base()
            {
                
            }
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task CanGetAllMetrics()
        {
            var client = new GraphiteClient(GraphiteHost);
            var metrics = await client.GetAllMetricsAsync();
            Assert.NotNull(metrics);
            Assert.NotEmpty(metrics);
            Assert.False(String.IsNullOrEmpty(metrics[0]));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task CanFindMetric()
        {
            var client = new GraphiteClient(GraphiteHost);
            var metrics = await client.FindMetricsAsync("usage.unittest.*");
            Assert.NotNull(metrics);
            Assert.NotEmpty(metrics);
            Assert.True(metrics.All(x=>x.Id.StartsWith("usage.unittest.")));
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task CanExpandMetrics()
        {
            var client = new GraphiteClient(GraphiteHost);
            var path1 = new GraphitePath("usage").Dot("unittest").Dot("iaas").DotWildcard().DotWildcard().DotWildcard();
            var path2 = new GraphitePath("usage").Dot("unittest").Dot("license").DotWildcard();
            var metrics = await client.ExpandMetricsAsync(path1, path2);
            Assert.NotNull(metrics);
            Assert.NotEmpty(metrics);
            Assert.True(metrics.All(x=>x.StartsWith("usage.unittest.")));
            Assert.Contains(metrics, x =>x.StartsWith("usage.unittest.iaas."));
            Assert.Contains(metrics, x =>x.StartsWith("usage.unittest.license."));
        }

        [Fact]
        public void CanDeserializeMetrics()
        {
            var json = "{\"is_leaf\": \"0\", \"name\": \"pickled\", \"path\": \"usage.unittest.pickled.\"}";
            var metric = JsonConvert.DeserializeObject<GraphiteMetric>(json);
            Assert.Equal("usage.unittest.pickled", metric.Id);
            Assert.Equal("pickled", metric.Text);
            Assert.False(metric.Leaf);
            Assert.True(metric.Expandable);
        }

        [Fact]
        public void CanDeserializeMetricsData()
        {
            var json = "{\"target\": \"usage.unittest.cpu.count\", \"datapoints\": [[3.5, 1474716420], [null, 1474716480], [null, 1474716540], [0, 1474716600], [7.0, 1474716660], [null, 1474716720], [null, 1474716780]]}";
            var data = JsonConvert.DeserializeObject<GraphiteMetricData>(json);
            Assert.Equal("usage.unittest.cpu.count", data.Target);
            Assert.NotNull(data.Datapoints);
            Assert.Equal(7,data.Datapoints.Length);
            var value = data.Datapoints[0];
            Assert.Equal(3.5, value.Value);
            Assert.Equal(1474716420, value.UnixTimestamp);
            Assert.Equal(new DateTime(2016, 09, 24, 11, 27, 0), value.Timestamp);
        }

        [Fact]
        public void CanSerialize()
        {
            var datapoint = new MetricDatapoint(null, 987654321);
            var json = JsonConvert.SerializeObject(datapoint);
            Assert.Equal("[null,987654321]", json);

            var datapoint2 = new MetricDatapoint(5, 123456789);
            json = JsonConvert.SerializeObject(datapoint2);
            Assert.Equal("[5.0,123456789]", json);

            var metricData = new GraphiteMetricData("unit.test", new[] { datapoint2, datapoint });
            json = JsonConvert.SerializeObject(metricData);
            Assert.Equal("{\"target\":\"unit.test\",\"datapoints\":[[5.0,123456789],[null,987654321]]}", json);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task CanGetMetricValues()
        {
            var client = new GraphiteClient(GraphiteHost);
            var metric = new GraphitePath("usage").Dot("unittest").Dot("iaas").DotWildcard().Dot("cpu").Dot("max");
            var data = await client.GetMetricsDataAsync(metric);
            Assert.NotNull(data);
            var series = data.FirstOrDefault();
            Assert.NotNull(series);
            Assert.NotNull(series.Datapoints);
            Assert.NotEmpty(series.Datapoints);
            Assert.Contains(series.Datapoints, x =>x.Value.HasValue);
            Assert.True(series.Datapoints.All(x=>x.Timestamp < DateTime.Now));
        }

        [Fact]
        public async Task CanQueryLongFormulas()
        {
            var client = new GraphiteClient(GraphiteHost){UseSsl = false};
            var metrics = new SeriesListBase[768];
            for (int i = 0; i < metrics.Length; i++)
            {
                metrics[i] = new GraphitePath("prefix").Dot("path")
                    .Dot(Guid.NewGuid().ToString().Replace("-", String.Empty))
                    .Dot("category")
                    .Dot(Guid.NewGuid().ToString().Replace("-", String.Empty))
                    .Dot("value")
                    .Alias(i.ToString());
            }
            var metric = metrics.Sum();
            Assert.True(metric.ToString().Length > UInt16.MaxValue, "request too short to fail");
            try
            {
                await client.GetMetricsDataAsync(metric);
            }
            catch (UriFormatException)
            {
                throw;
            }
            catch
            {
                // ignored host may be not reachable
            }
        }
    }
}
