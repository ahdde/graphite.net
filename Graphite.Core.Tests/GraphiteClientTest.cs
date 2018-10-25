using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ahd.Graphite;
using Newtonsoft.Json;
using Razorvine.Pickle;
using Xunit;

namespace Graphite.Core.Tests
{
    public class GraphiteClientTest
    {
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
            const string host = "example.com";
            const ushort port = 2004;
            var client = new GraphiteClient(host);
            Assert.Equal(host, client.Host);

            client = new GraphiteClient(host, new TestGraphiteFormatter(port));
            Assert.Equal(host, client.Host);
            Assert.Equal(port, client.Formatter.Port);
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
            Assert.True(metric.Contains("usage.unittest.cpu.count"));
            server.Stop();
            Console.WriteLine(metric);
        }
        
        [Fact]
        public async Task CanSendPickledMetric()
        {
            var server = new TcpListener(new IPEndPoint(IPAddress.Loopback, 33225));
            server.Start();
            var client = new GraphiteClient("localhost", new PickleGraphiteFormatter(33225));
            var recvTask = ReceiveMetric(server);
            var sendTask = client.SendAsync("usage.unittest.cpu.count", Environment.ProcessorCount);
            await sendTask;
            var metric = await recvTask;
            Assert.True(metric.Contains("usage.unittest.cpu.count"));
            server.Stop();
            Console.WriteLine(metric);
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
        public void DataPointConversion()
        {
            var data = new Datapoint("test", 0, 1451338593);
            Assert.Equal(1451338593,data.UnixTimestamp);
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
    }
}