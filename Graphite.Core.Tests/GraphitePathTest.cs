using ahd.Graphite.Base;
using Xunit;

namespace Graphite.Core.Tests
{
    public class GraphitePathTest
    {
        [Fact]
        public void GraphitePath()
        {
            var path = new GraphitePath("metric");
            Assert.Equal("metric", path.Name);
            Assert.Equal("metric", path.ToString());
        }

        [Fact]
        public void CombineNameTest()
        {
            var path = new GraphitePath("metric");
            Assert.Equal("metric", path.ToString());
            path = path.Dot("used");
            Assert.Equal("metric.used", path.ToString());
            path = path.Wildcard();
            Assert.Equal("metric.used*", path.ToString());
            path = path.Wildcard();
            Assert.Equal("metric.used*", path.ToString());
            path = path.DotWildcard();
            Assert.Equal("metric.used*.*", path.ToString());
            path = path.Range('a', 'z');
            Assert.Equal("metric.used*.*[a-z]", path.ToString());
            path = path.Range('0', '9');
            Assert.Equal("metric.used*.*[a-z0-9]", path.ToString());
            path = path.DotRange('0', '9');
            Assert.Equal("metric.used*.*[a-z0-9].[0-9]", path.ToString());
            path = path.Chars('a', 'd', 'f');
            Assert.Equal("metric.used*.*[a-z0-9].[0-9][adf]", path.ToString());
            path = path.Chars('q');
            Assert.Equal("metric.used*.*[a-z0-9].[0-9][adfq]", path.ToString());
            path = path.DotChars('w');
            Assert.Equal("metric.used*.*[a-z0-9].[0-9][adfq].[w]", path.ToString());
            path = path.Values("asdf");
            Assert.Equal("metric.used*.*[a-z0-9].[0-9][adfq].[w]{asdf}", path.ToString());
            path = path.Values("qwertz");
            Assert.Equal("metric.used*.*[a-z0-9].[0-9][adfq].[w]{asdf,qwertz}", path.ToString());
            path = path.DotValues("01", "02", "03");
            Assert.Equal("metric.used*.*[a-z0-9].[0-9][adfq].[w]{asdf,qwertz}.{01,02,03}", path.ToString());
        }

        [Fact]
        public void AlternatingTest()
        {
            var path = new GraphitePath("dummy");
            path = path.Chars('a', 'b', 'd').Values("a", "v", "b").Chars('z', 'y', 'x');
            Assert.Equal("dummy[abd]{a,v,b}[zyx]", path.ToString());
        }

        [Fact]
        public void WildcardTest()
        {
            var metric = new GraphitePath("usage").Dot("unittest").DotWildcard().Dot("count");
            Assert.Equal("usage.unittest.*.count", metric.ToString());
        }
    }
}