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
            path = path.Dot("used");
            Assert.AreEqual("metric.used", path.ToString());
            path = path.Wildcard();
            Assert.AreEqual("metric.used*", path.ToString());
            path = path.Wildcard();
            Assert.AreEqual("metric.used*", path.ToString());
            path = path.DotWildcard();
            Assert.AreEqual("metric.used*.*", path.ToString());
            path = path.Range('a', 'z');
            Assert.AreEqual("metric.used*.*[a-z]", path.ToString());
            path = path.Range('0', '9');
            Assert.AreEqual("metric.used*.*[a-z0-9]", path.ToString());
            path = path.DotRange('0', '9');
            Assert.AreEqual("metric.used*.*[a-z0-9].[0-9]", path.ToString());
            path = path.Chars('a', 'd', 'f');
            Assert.AreEqual("metric.used*.*[a-z0-9].[0-9][adf]", path.ToString());
            path = path.Chars('q');
            Assert.AreEqual("metric.used*.*[a-z0-9].[0-9][adfq]", path.ToString());
            path = path.DotChars('w');
            Assert.AreEqual("metric.used*.*[a-z0-9].[0-9][adfq].[w]", path.ToString());
            path = path.Values("asdf");
            Assert.AreEqual("metric.used*.*[a-z0-9].[0-9][adfq].[w]{asdf}", path.ToString());
            path = path.Values("qwertz");
            Assert.AreEqual("metric.used*.*[a-z0-9].[0-9][adfq].[w]{asdf,qwertz}", path.ToString());
            path = path.DotValues("01","02","03");
            Assert.AreEqual("metric.used*.*[a-z0-9].[0-9][adfq].[w]{asdf,qwertz}.{01,02,03}", path.ToString());
        }

        [TestMethod]
        public void AlternatingTest()
        {
            var path = new GraphitePath("dummy");
            path = path.Chars('a', 'b', 'd').Values("a","v","b").Chars('z','y','x');
            Assert.AreEqual("dummy[abd]{a,v,b}[zyx]", path.ToString());
        }

        [TestMethod]
        public void WildcardTest()
        {
            var metric = new GraphitePath("usage").Dot("unittest").DotWildcard().Dot("count");
            Assert.AreEqual("usage.unittest.*.count", metric.ToString());
        }
    }
}