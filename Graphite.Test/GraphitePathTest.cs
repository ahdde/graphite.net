using ahd.Graphite.Base;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ahd.Graphite.Test
{
    [TestClass]
    public class GraphitePathTest
    {
        [TestMethod]
        public void GraphitePath()
        {
            var path = new GraphitePath("metric");
            Assert.AreEqual("metric", path.Name);
            Assert.AreEqual("metric", path.ToString());
        }

        [TestMethod]
        public void CombineNameTest()
        {
            var path = new GraphitePath("metric");
            Assert.AreEqual("metric", path.ToString());
            path = path.Path("used");
            Assert.AreEqual("metric.used", path.ToString());
            path = path.Wildcard();
            Assert.AreEqual("metric.used*", path.ToString());
            path = path.Wildcard();
            Assert.AreEqual("metric.used*", path.ToString());
            path = path.WildcardPath();
            Assert.AreEqual("metric.used*.*", path.ToString());
            path = path.Range('a', 'z');
            Assert.AreEqual("metric.used*.*[a-z]", path.ToString());
            path = path.Range('0', '9');
            Assert.AreEqual("metric.used*.*[a-z0-9]", path.ToString());
            path = path.RangePath('0', '9');
            Assert.AreEqual("metric.used*.*[a-z0-9].[0-9]", path.ToString());
            path = path.Chars('a', 'd', 'f');
            Assert.AreEqual("metric.used*.*[a-z0-9].[0-9][adf]", path.ToString());
            path = path.Chars('q');
            Assert.AreEqual("metric.used*.*[a-z0-9].[0-9][adfq]", path.ToString());
            path = path.CharsPath('w');
            Assert.AreEqual("metric.used*.*[a-z0-9].[0-9][adfq].[w]", path.ToString());
            path = path.Value("asdf");
            Assert.AreEqual("metric.used*.*[a-z0-9].[0-9][adfq].[w]{asdf}", path.ToString());
            path = path.Value("qwertz");
            Assert.AreEqual("metric.used*.*[a-z0-9].[0-9][adfq].[w]{asdf,qwertz}", path.ToString());
            path = path.ValuePath("01","02","03");
            Assert.AreEqual("metric.used*.*[a-z0-9].[0-9][adfq].[w]{asdf,qwertz}.{01,02,03}", path.ToString());
        }

        [TestMethod]
        public void AlternatingTest()
        {
            var path = new GraphitePath("dummy");
            path = path.Chars('a', 'b', 'd').Value("a","v","b").Chars('z','y','x');
            Assert.AreEqual("dummy[abd]{a,v,b}[zyx]", path.ToString());
        }

        [TestMethod]
        public void WildcardTest()
        {
            var metric = new GraphitePath("usage").Path("unittest").WildcardPath().Path("count");
            Assert.AreEqual("usage.unittest.*.count", metric.ToString());
        }
    }
}