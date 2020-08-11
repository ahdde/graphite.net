using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ahd.Graphite
{
    internal static class HttpClientExtensions
    {
        internal static async Task<T> ReadAsAsync<T>(this HttpContent content, CancellationToken cancellationToken)
        {
            using (var stream = await content.ReadAsStreamAsync().ConfigureAwait(false))
            {
                return await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: cancellationToken);
            }
        }
    }
}
