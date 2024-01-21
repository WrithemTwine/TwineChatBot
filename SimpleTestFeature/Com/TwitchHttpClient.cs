using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleTestFeature.Com
{
    internal static class TwitchHttpClient
    {
        private static HttpClient httpClient = new();

        public static async Task<HttpResponseMessage> DeleteAsync(Uri url)
        {
            return await httpClient.DeleteAsync(url);
        }

        public static async Task<HttpResponseMessage> GetAsync(Uri url, HttpRequestMessage content = null)
        {
            if (content == null)
            {
                return await httpClient.GetAsync(url);
            }
            else
            {
                content.RequestUri = url;

                return await httpClient.SendAsync(content).ConfigureAwait(false);
            }
        }

        public static async Task<HttpResponseMessage> PostAsync(Uri url, HttpContent body)
        {
            return await httpClient.PostAsync(url, body);
        }

        public static async Task<HttpResponseMessage> PutAsync(Uri url, HttpContent body)
        {
            return await httpClient.PutAsync(url, body);
        }
    }
}
