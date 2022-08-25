using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using SteamAuthCore.Exceptions;

namespace SteamAuthCore.Obsolete
{
    [Obsolete]
    public static class SteamApi
    {
        public enum RequestMethod
        {
            Get,
            Post,
        }

        public static Task<string?> MobileLoginRequest(string url, RequestMethod method, NameValueCollection? data = null, CookieContainer? cookies = null, NameValueCollection? headers = null)
        {
            return RequestAsync(url, method, data, cookies, headers, ApiEndpoints.CommunityBase + "/mobilelogin?oauth_client_id=DE45CD61&oauth_scope=read_profile%20write_profile%20read_client%20write_client");
        }

        public static Task<T?> MobileLoginRequest<T>(string url, RequestMethod method, NameValueCollection? data = null, CookieContainer? cookies = null, NameValueCollection? headers = null)
        {
            return RequestAsync<T>(url, method, data, cookies, headers, ApiEndpoints.CommunityBase + "/mobilelogin?oauth_client_id=DE45CD61&oauth_scope=read_profile%20write_profile%20read_client%20write_client");
        }

        public static T? Request<T>(string url, RequestMethod method, string dataString, CookieContainer? cookies = null, NameValueCollection? headers = null, string referer = ApiEndpoints.CommunityBase)
        {
            var request = CreateHttpWebRequest(url, method, cookies, headers, referer);

            if (method == RequestMethod.Post)
            {
                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                request.ContentLength = dataString.Length;

                StreamWriter requestStream = new(request.GetRequestStream());
                requestStream.Write(dataString);
                requestStream.Close();
            }

            try
            {
                using var response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    HandleFailedWebRequestResponse(response, url);
                    return default;
                }

                return JsonSerializer.Deserialize<T>(response.GetResponseStream() ??
                                                     throw new InvalidOperationException());
            }
            catch (WebException e)
            {
                HandleFailedWebRequestResponse(e.Response as HttpWebResponse ?? throw new InvalidOperationException(),
                    url);
                return default;
            }
            catch
            {
                return default;
            }
        }


        public static async Task<T?> RequestAsync<T>(string url, RequestMethod method, NameValueCollection? data = null, CookieContainer? cookies = null, NameValueCollection? headers = null, string referer = ApiEndpoints.CommunityBase)
        {
            var query = CreateQuery(data);
            return await RequestAsync<T>(url, method, query, cookies, headers, referer);
        }

        public static async Task<T?> RequestAsync<T>(string url, RequestMethod method, string query, CookieContainer? cookies = null, NameValueCollection? headers = null, string referer = ApiEndpoints.CommunityBase)
        {
            if (method == RequestMethod.Get)
                url += (url.Contains("?") ? "&" : "?") + query;

            var request = CreateHttpWebRequest(url, method, cookies, headers, referer);

            if (method == RequestMethod.Post)
                request = await WritePostData(request, query);

            try
            {
                using var response = (HttpWebResponse)await request.GetResponseAsync();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    HandleFailedWebRequestResponse(response, url);
                    return default;
                }

                var result = await JsonSerializer.DeserializeAsync<T>(response.GetResponseStream() ?? throw new InvalidOperationException());
                return result;
            }
            catch (WebException e)
            {
                HandleFailedWebRequestResponse(e.Response as HttpWebResponse, url);
                return default;
            }
            catch
            {
                return default;
            }
        }


        public static async Task<string?> RequestAsync(string url, RequestMethod method, NameValueCollection? data = null, CookieContainer? cookies = null, NameValueCollection? headers = null, string referer = ApiEndpoints.CommunityBase)
        {
            var query = CreateQuery(data);
            return await RequestAsync(url, method, query, cookies, headers, referer);
        }

        public static async Task<string?> RequestAsync(string url, RequestMethod method, string query, CookieContainer? cookies = null, NameValueCollection? headers = null, string referer = ApiEndpoints.CommunityBase)
        {
            if (method == RequestMethod.Get)
                url += (url.Contains("?") ? "&" : "?") + query;

            var request = CreateHttpWebRequest(url, method, cookies, headers, referer);

            if (method == RequestMethod.Post)
                request = await WritePostData(request, query);

            try
            {
                using var response = (HttpWebResponse)await request.GetResponseAsync();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    HandleFailedWebRequestResponse(response, url);
                    return null;
                }

                using StreamReader responseStream =
                    new(response.GetResponseStream() ?? throw new InvalidOperationException());
                var responseData = await responseStream.ReadToEndAsync();
                return responseData;
            }
            catch (WebException e)
            {
                HandleFailedWebRequestResponse(e.Response as HttpWebResponse, url);
                return null;
            }
            catch
            {
                return null;
            }
        }


        private static HttpWebRequest CreateHttpWebRequest(string url, RequestMethod method, CookieContainer? cookies = null, NameValueCollection? headers = null, string referer = ApiEndpoints.CommunityBase)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = method.ToString().ToUpper();
            request.Accept = "text/javascript, text/html, application/xml, text/xml, */*";
            request.UserAgent = "Mozilla/5.0 (Linux; U; Android 4.1.1; en-us; Google Nexus 4 - 4.1.1 - API 16 - 768x1280 Build/JRO03S) AppleWebKit/534.30 (KHTML, like Gecko) Version/4.0 Mobile Safari/534.30";
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            request.Referer = referer;

            if (headers != null)
                request.Headers.Add(headers);

            if (cookies != null)
                request.CookieContainer = cookies;

            return request;
        }

        private static void HandleFailedWebRequestResponse(HttpWebResponse? response, string requestUrl)
        {
            if (response is null) return;

            //Redirecting -- likely to a steammobile:// URI
            if (response.StatusCode != HttpStatusCode.Found) return;

            var location = response.Headers.Get("Location");
            if (string.IsNullOrEmpty(location)) return;

            //Our OAuth token has expired. This is given both when we must refresh our session, or the entire OAuth Token cannot be refreshed anymore.
            //Thus, we should only throw this exception when we're attempting to refresh our session.
            if (location == "steammobile://lostauth" && requestUrl == ApiEndpoints.MobileauthGetwgtoken)
                throw new WgTokenExpiredException();
        }

        private static string CreateQuery(NameValueCollection? data) => data == null
            ? string.Empty
            : string.Join("&",
                Array.ConvertAll(data.AllKeys,
                    key => $"{WebUtility.UrlEncode(key)}={WebUtility.UrlEncode(data[key])}"));

        private static async Task<HttpWebRequest> WritePostData(HttpWebRequest request, string query)
        {
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            request.ContentLength = query.Length;

            using StreamWriter requestStream = new(request.GetRequestStream());
            await requestStream.WriteAsync(query);

            return request;
        }
    }
}
