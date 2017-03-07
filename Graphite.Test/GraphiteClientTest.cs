using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ahd.Graphite.Base;
using ahd.Graphite.Test.Properties;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Razorvine.Pickle;

namespace ahd.Graphite.Test
{
    [TestClass]
    public class GraphiteClientTest
    {
        [TestMethod]
        public void CanCreateClient()
        {
            var client = new GraphiteClient();
        }

        [TestMethod]
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

        [TestMethod]
        public void ClientHasSaneDefaultValues()
        {
            var client = new GraphiteClient();
            Assert.AreEqual(2003, client.Formatter.Port);
            Assert.IsInstanceOfType(client.Formatter, typeof(PlaintextGraphiteFormatter));
            Assert.AreEqual("localhost", client.Host);
        }

        [TestMethod]
        public void CanCreateClientWithParams()
        {
            var client = new GraphiteClient(Settings.Default.GraphiteHost);
            Assert.AreEqual(Settings.Default.GraphiteHost, client.Host);

            client = new GraphiteClient(Settings.Default.GraphiteHost, new TestGraphiteFormatter(2004));
            Assert.AreEqual(Settings.Default.GraphiteHost, client.Host);
            Assert.AreEqual(2004, client.Formatter.Port);
            Assert.IsInstanceOfType(client.Formatter, typeof(TestGraphiteFormatter));
        }

        [TestMethod]
        public async Task CanSendMetric()
        {
            var server = new TcpListener(new IPEndPoint(IPAddress.Loopback, 33225));
            server.Start();
            var client = new GraphiteClient("localhost", new PlaintextGraphiteFormatter(33225));
            var recvTask = ReceiveMetric(server);
            var sendTask = client.SendAsync("usage.unittest.cpu.count", Environment.ProcessorCount);
            await sendTask;
            var metric = await recvTask;
            Assert.IsTrue(metric.Contains("usage.unittest.cpu.count"));
            server.Stop();
            Console.WriteLine(metric);
        }

        [TestMethod]
        public void DataPointConversion()
        {
            var data = new Datapoint("test", 0, 1451338593);
            Assert.AreEqual(1451338593,data.UnixTimestamp);
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
        
        [TestMethod]
        [TestCategory("Integration")]
        public async Task SendMetric()
        {
            var client = new GraphiteClient(Settings.Default.GraphiteHost);
            await client.SendAsync("usage.unittest.cpu.count", Environment.ProcessorCount);
            Console.WriteLine("done");
        }
        
        [TestMethod]
        [TestCategory("Integration")]
        public async Task SendPickledMetric()
        {
            var client = new GraphiteClient(Settings.Default.GraphiteHost, new PickleGraphiteFormatter());
            await client.SendAsync("usage.unittest.pickled.cpu.count", Environment.ProcessorCount);
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

        [TestMethod]
        [TestCategory("Integration")]
        public async Task CanGetAllMetrics()
        {
            var client = new GraphiteClient(Settings.Default.GraphiteHost);
            var metrics = await client.GetAllMetricsAsync();
            Assert.IsNotNull(metrics);
            Assert.AreNotEqual(0, metrics.Length);
            Assert.IsFalse(String.IsNullOrEmpty(metrics[0]));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task CanFindMetric()
        {
            var client = new GraphiteClient(Settings.Default.GraphiteHost);
            var metrics = await client.FindMetricsAsync("usage.unittest.*");
            Assert.IsNotNull(metrics);
            Assert.AreNotEqual(0, metrics.Length);
            Assert.IsTrue(metrics.All(x=>x.Id.StartsWith("usage.unittest.")));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task CanExpandMetrics()
        {
            var client = new GraphiteClient(Settings.Default.GraphiteHost);
            var path1 = new GraphitePath("usage").Path("unittest").Path("iaas").WildcardPath().WildcardPath().WildcardPath();
            var path2 = new GraphitePath("usage").Path("unittest").Path("license").WildcardPath();
            var metrics = await client.ExpandMetricsAsync(path1, path2);
            Assert.IsNotNull(metrics);
            Assert.AreNotEqual(0, metrics.Length);
            Assert.IsTrue(metrics.All(x=>x.StartsWith("usage.unittest.")));
            Assert.IsTrue(metrics.Any(x=>x.StartsWith("usage.unittest.iaas.")));
            Assert.IsTrue(metrics.Any(x=>x.StartsWith("usage.unittest.license.")));
        }

        [TestMethod]
        public void CanDeserializeMetrics()
        {
            var json = "{\"is_leaf\": \"0\", \"name\": \"pickled\", \"path\": \"usage.unittest.pickled.\"}";
            var metric = JsonConvert.DeserializeObject<GraphiteMetric>(json);
            Assert.AreEqual("usage.unittest.pickled", metric.Id);
            Assert.AreEqual("pickled", metric.Text);
            Assert.IsFalse(metric.Leaf);
            Assert.IsTrue(metric.Expandable);
        }

        [TestMethod]
        public void CanDeserializeMetricsData()
        {
            var json = "{\"target\": \"usage.unittest.cpu.count\", \"datapoints\": [[3.5, 1474716420], [null, 1474716480], [null, 1474716540], [null, 1474716600], [7.0, 1474716660], [null, 1474716720], [null, 1474716780]]}";
            var data = JsonConvert.DeserializeObject<GraphiteMetricData>(json);
            Assert.AreEqual("usage.unittest.cpu.count", data.Target);
            Assert.IsNotNull(data.Datapoints);
            Assert.AreEqual(7,data.Datapoints.Length);
            var value = data.Datapoints[0];
            Assert.AreEqual(3.5, value.Value);
            Assert.AreEqual(1474716420, value.UnixTimestamp);
            Assert.AreEqual(new DateTime(2016, 09, 24, 11, 27, 0), value.Timestamp);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public async Task CanGetMetricValues()
        {
            var client = new GraphiteClient(Settings.Default.GraphiteHost);
            var metric = new GraphitePath("usage").Path("unittest").Path("iaas").WildcardPath().Path("cpu").Path("max");
            var data = await client.GetMetricsDataAsync(metric);
            Assert.IsNotNull(data);
            var series = data.FirstOrDefault();
            Assert.IsNotNull(series);
            Assert.IsNotNull(series.Datapoints);
            Assert.AreNotEqual(0, series.Datapoints.Length);
            Assert.IsTrue(series.Datapoints.Any(x=>x.Value.HasValue));
            Assert.IsTrue(series.Datapoints.All(x=>x.Timestamp < DateTime.Now));
        }
    }
}
