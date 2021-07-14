using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ahd.Graphite.Base;
using ahd.Graphite.Exceptions;
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
        [Trait("Category", "Integration")]
        public async Task CanSerialiseHttpContent()
        {
            var json = "{\"is_leaf\": \"0\", \"name\": \"pickled\", \"path\": \"usage.unittest.pickled.\"}";
            var client = new HttpClient();
            var response = await client.GetAsync("http://echo.jsontest.com/is_leaf/0/name/pickled/path/usage.unittest.pickled./");
            await response.EnsureSuccessStatusCodeAsync();
            var read = await response.Content.ReadAsAsync<GraphiteMetric>(CancellationToken.None);
            var des = JsonSerializer.Deserialize<GraphiteMetric>(json);
            Assert.Equal(des.Text, read.Text);
            Assert.Equal(des.Expandable, read.Expandable);
            Assert.Equal(des.Id, read.Id);
            Assert.Equal(des.Leaf, read.Leaf);
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
            new CarbonConnectionPool("localhost", new PickleGraphiteFormatter(33225)).ClearPool();
            var metric = await recvTask;
            Assert.Contains("usage.unittest.cpu.count", metric);
            server.Stop();
            Console.WriteLine(metric);
        }

        [Fact]
        public async Task CanReusePooledConnectionMetric()
        {
            var server = new TcpListener(new IPEndPoint(IPAddress.Loopback, 33225));
            server.Start();
            var client = new GraphiteClient("localhost", new PlaintextGraphiteFormatter(33225));
            var recvTask = ReceiveMetric(server);
            await client.SendAsync("usage.unittest.cpu.count", Environment.ProcessorCount);
            
            client = new GraphiteClient("localhost", new PlaintextGraphiteFormatter(33225));
            await client.SendAsync("usage.unittest.ram.count", Environment.ProcessorCount);
            //"kill" all existing tcp connections
            new CarbonConnectionPool("localhost", new PickleGraphiteFormatter(33225)).ClearPool();

            var metric = await recvTask;
            Assert.Contains("usage.unittest.cpu.count", metric);
            Assert.Contains("usage.unittest.ram.count", metric);
            server.Stop();
            Console.WriteLine(metric);
        }

        [Fact]
        public async Task BrokenPooledConnectionIsDetected()
        {
            var server = new TcpListener(new IPEndPoint(IPAddress.Loopback, 33225));
            server.Start();
            var client = new GraphiteClient("localhost", new PlaintextGraphiteFormatter(33225));

            var send = client.SendAsync("usage.unittest.cpu.count", Environment.ProcessorCount);
            //accept and dispose client connection
            using (var conn = await server.AcceptTcpClientAsync())
            using (var stream = conn.GetStream())
            using (var reader = new StreamReader(stream))
            { 
                var receive =reader.ReadLineAsync();
                await send;
                await receive;
            }
            client = new GraphiteClient("localhost", new PlaintextGraphiteFormatter(33225));
            var recvTask = ReceiveMetric(server);
            await client.SendAsync("usage.unittest.cpu.count", Environment.ProcessorCount);
            new CarbonConnectionPool("localhost", new PickleGraphiteFormatter(33225)).ClearPool();

            var metric = await recvTask;
            Assert.Contains("usage.unittest.cpu.count", metric);
            server.Stop();
            Console.WriteLine(metric);
        }

        [Fact]
        public async Task BrokenPooledConnectionIsDetectedForPickle()
        {
            var server = new TcpListener(new IPEndPoint(IPAddress.Loopback, 33225));
            server.Start();
            var client = new GraphiteClient("localhost", new PickleGraphiteFormatter(33225));

            var send = client.SendAsync("usage.unittest.cpu.count", Environment.ProcessorCount); 
            //accept and dispose client connection
            using (var conn = await server.AcceptTcpClientAsync())
            using (var stream = conn.GetStream())
            using (var reader = new StreamReader(stream))
            { 
                var receive =reader.ReadAsync(new char[1], 0, 1);
                await send;
                await receive;
            }
            client = new GraphiteClient("localhost", new PickleGraphiteFormatter(33225));
            var recvTask = ReceiveMetric(server);
            await client.SendAsync("usage.unittest.cpu.count", Environment.ProcessorCount);
            new CarbonConnectionPool("localhost", new PickleGraphiteFormatter(33225)).ClearPool();

            var metric = await recvTask;
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

        [Fact]
        [Trait("Category", "Integration")]
        public void CanSendManyMetrics()
        {
            var random = new Random();
            while (random.Next() > 0)
            {
                var client = new GraphiteClient(GraphiteHost, new PickleGraphiteFormatter());
                client.Send("test.client.random1", random.NextDouble() * 100);
                client = new GraphiteClient(GraphiteHost, new PlaintextGraphiteFormatter());
                client.Send("test.client.random2", random.NextDouble() * 100);
                Thread.Sleep(500);
            }
            CarbonConnectionPool.ClearAllPools();
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task CanSendManyMetricsAsync()
        {
            var random = new Random();
            while (random.Next() > 0)
            {
                var client = new GraphiteClient(GraphiteHost, new PickleGraphiteFormatter());
                await client.SendAsync("test.client.random1", random.NextDouble() * 100);
                client = new GraphiteClient(GraphiteHost, new PlaintextGraphiteFormatter());
                await client.SendAsync("test.client.random2", random.NextDouble() * 100);
                await Task.Delay(500);
            }
            CarbonConnectionPool.ClearAllPools();
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
            var metric = JsonSerializer.Deserialize<GraphiteMetric>(json);
            Assert.Equal("usage.unittest.pickled", metric.Id);
            Assert.Equal("pickled", metric.Text);
            Assert.False(metric.Leaf);
            Assert.True(metric.Expandable);
        }

        [Fact]
        public void CanDeserializeMetricDatapoint()
        {
            var json = "[3.5, 1474716420]";
            var dp = JsonSerializer.Deserialize<MetricDatapoint>(json);
            Assert.Equal(3.5D, dp.Value);
            Assert.Equal(1474716420L, dp.UnixTimestamp);

            json = "[null, 1474716424]";
            dp = JsonSerializer.Deserialize<MetricDatapoint>(json);
            Assert.Null(dp.Value);
            Assert.Equal(1474716424, dp.UnixTimestamp);
        }

        [Fact]
        public void DeserializationThrowsOnIllegalJson()
        {
            var json = "{Test: 24, Array: [3.5, null]}";
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<MetricDatapoint>(json));
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<GraphiteMetric>(json));
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<GraphiteMetricData>(json));
        }

        [Fact]
        public void CanDeserializeMetricsData()
        {
            var json = "{\"target\": \"usage.unittest.cpu.count\", \"datapoints\": [[3.5, 1474716420], [null, 1474716480], [null, 1474716540], [0, 1474716600], [7.0, 1474716660], [null, 1474716720], [null, 1474716780]]}";
            var data = JsonSerializer.Deserialize<GraphiteMetricData>(json);
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
            var json = JsonSerializer.Serialize(datapoint);
            Assert.Equal("[null,987654321]", json);

            var datapoint2 = new MetricDatapoint(5, 123456789);
            json = JsonSerializer.Serialize(datapoint2);
            Assert.Equal("[5,123456789]", json);

            var metricData = new GraphiteMetricData("unit.test", new[] { datapoint2, datapoint });
            json = JsonSerializer.Serialize(metricData);
            Assert.Equal("{\"target\":\"unit.test\",\"datapoints\":[[5,123456789],[null,987654321]]}", json);
        }

        [Fact]
        public void CanDeserialize()
        {
            var json = "[5,123456789]";
            var datapoint = JsonSerializer.Deserialize<MetricDatapoint>(json);
            Assert.Equal(123456789, datapoint.UnixTimestamp);
            Assert.Equal(5, datapoint.Value);

            json = "{\"target\":\"unit.test\",\"datapoints\":[[5,123456789],[null,987654321]]}";
            var metricData = JsonSerializer.Deserialize<GraphiteMetricData>(json);
            Assert.Equal("unit.test", metricData.Target);
            Assert.Equal(2, metricData.Datapoints.Length);

            json = "{\"results\":[\"usage.test.1\",\"usage.test.2\"]}";
            var expandResult = JsonSerializer.Deserialize<GraphiteExpandResult>(json);
            Assert.Equal(2, expandResult.Results.Length);

            json = "{\"metrics\":[{\"is_leaf\":\"0\",\"name\":\"1\",\"path\":\"usage.unittest.1.\"},{\"is_leaf\":\"1\",\"name\":\"2\",\"path\":\"usage.unittest.2\"}]}";
            var findResult = JsonSerializer.Deserialize<GraphiteFindResult>(json);
            Assert.Equal(2, findResult.Metrics.Length);
            Assert.True(findResult.Metrics[1].Leaf);
            Assert.Equal("1", findResult.Metrics[0].Text);
            Assert.Equal("usage.unittest.1", findResult.Metrics[0].Id);
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
