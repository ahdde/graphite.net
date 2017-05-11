using System.Net.Http;
using System.Threading.Tasks;

namespace ahd.Graphite.Exceptions
{
    public static class HttpResponseMessageExtension
    {
        public static async Task EnsureSuccessStatusCodeAsync(this HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            using (response.Content)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(response.StatusCode, content);
            }
        }
    }
}