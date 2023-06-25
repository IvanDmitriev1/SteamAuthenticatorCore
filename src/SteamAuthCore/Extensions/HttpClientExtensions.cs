namespace SteamAuthCore.Extensions;

internal static class HttpClientExtensions
{
    public static void AddDefaultHeaders(this HttpClient client)
    {
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));
        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36");
        //client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate");
    }
}
