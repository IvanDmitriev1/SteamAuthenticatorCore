using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace WpfHelper.Services
{
    public static class SimpleHttpRequestService
    {
        #region HelpClasses

        public struct RequestHeader
        {
            public string Name { get; init; }
            public string Value { get; init; }
        }

        #endregion

        public static HttpClient CreateRequest(string url, RequestHeader[]? requestHeader = null, MediaTypeWithQualityHeaderValue[]? acceptHeaders = null)
        {
            HttpClient client = new()
            {
                BaseAddress = new Uri(url, UriKind.Absolute)
            };

            if (requestHeader is null)
            {
                client.DefaultRequestHeaders.Add("User-Agent", "User");
            }
            else
            {
                foreach (var value in requestHeader)
                {
                    client.DefaultRequestHeaders.Add(value.Name, value.Value);
                }
            }


            if (acceptHeaders is null)
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }
            else
            {
                foreach (var value in acceptHeaders)
                {
                    client.DefaultRequestHeaders.Accept.Add(value);
                }
            }

            return client;
        }

        public static async Task<HttpResponseMessage> CreateResponse(string url, RequestHeader[]? requestHeader = null, MediaTypeWithQualityHeaderValue[]? acceptHeaders = null)
        {
            using HttpClient client = CreateRequest(url);

            HttpResponseMessage response = await client.GetAsync(client.BaseAddress);
            response.EnsureSuccessStatusCode();

            return response;
        }
    }
}
