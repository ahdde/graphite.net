using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ahd.Graphite
{
    internal static class HttpClientExtensions
    {
        private static readonly JsonSerializer Serializer = new JsonSerializer();

        internal static async Task<T> ReadAsAsync<T>(this HttpContent content, CancellationToken cancellationToken)
        {
            using (var stream = await content.ReadAsStreamAsync().ConfigureAwait(false))
            {
                cancellationToken.ThrowIfCancellationRequested();
                using (var sReader = new StreamReader(stream))
                using (var jReader = new JsonTextReader(sReader))
                {
                    return Serializer.Deserialize<T>(jReader);
                }
            }
        }
    }
}
