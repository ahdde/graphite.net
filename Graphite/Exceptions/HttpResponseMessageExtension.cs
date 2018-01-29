using System.Net.Http;
using System.Threading.Tasks;

namespace ahd.Graphite.Exceptions
{
    /// <summary>
    /// Extensions for <see cref="HttpResponseMessage"/>
    /// </summary>
    public static class HttpResponseMessageExtension
    {
        /// <summary>
        /// checks the statuscode and throws a <see cref="HttpRequestException"/> with the full response content
        /// </summary>
        public static async Task EnsureSuccessStatusCodeAsync(this HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            using (response.Content)
            {
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new HttpRequestException(response.StatusCode, content);
            }
        }
    }
}