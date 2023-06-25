namespace SteamAuthCore.Services;

internal sealed class LegacySteamCommunityApi : ILegacySteamCommunityApi
{
    public LegacySteamCommunityApi(HttpClient client)
    {
        _client = client;

        _client.BaseAddress = new Uri(ApiEndpoints.CommunityBase);
        _client.AddDefaultHeaders();
        _client.DefaultRequestHeaders.Referrer = new Uri(ApiEndpoints.CommunityBase);

        _cookieStringForSessionId = CreateCookieForSessionId();
    }

    private readonly HttpClient _client;
    private readonly string _cookieStringForSessionId;

    private static string CreateCookieForSessionId()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("mobileClientVersion=0 (2.1.3);");
        builder.Append("mobileClient=android;");
        builder.Append("Steam_Language=english;");

        return builder.ToString();
    }

    public async Task<string?> GenerateSessionIdCookieForLogin(CancellationToken cancellationToken)
    {
        string url = "mobilelogin?" + ApiEndpoints.LoginOauth;

        using var message = new HttpRequestMessage(HttpMethod.Get, url);
        message.Headers.Referrer = new Uri(ApiEndpoints.MobileLoginRefer);
        message.Headers.Add("Cookie", _cookieStringForSessionId);
        message.Headers.Add("X-Requested-With", "com.valvesoftware.android.steam.community");

        using var responseMessage = await _client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!responseMessage.IsSuccessStatusCode)
            return null;

        if (!responseMessage.Headers.TryGetValues("Set-Cookie", out var cookies)) 
            return null;

        var sessionIdString = cookies.FirstOrDefault(s => s.Contains("sessionid"))!;
        return GetCookieValue(sessionIdString);
    }

    public async Task<RsaResponse?> LoginGetRsaKey(string userName, CancellationToken cancellationToken)
    {
        KeyValuePair<string, string>[] postData =
        {
            new("donotcache", $"{ITimeAligner.SteamTime * 1000}"),
            new("username", userName)
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, ApiEndpoints.LoginGetRSAKey);
        message.Headers.Referrer = new Uri(ApiEndpoints.MobileLoginRefer);
        message.Content = new FormUrlEncodedContent(postData);
        message.Headers.Add("Cookie", _cookieStringForSessionId);

        using var responseMessage = await _client.SendAsync(message, cancellationToken: cancellationToken);
        if (!responseMessage.IsSuccessStatusCode)
            return null;

        return await responseMessage.Content.ReadFromJsonAsync<RsaResponse>(cancellationToken: cancellationToken);
    }

    public async Task<DoLoginResult?> DoLogin(KeyValuePair<string, string>[] postData, string cookieString, CancellationToken cancellationToken)
    {
        string fullCookieString = _cookieStringForSessionId + cookieString;

        using var message = new HttpRequestMessage(HttpMethod.Post, ApiEndpoints.DoLogin);
        message.Headers.Referrer = new Uri(ApiEndpoints.MobileLoginRefer);
        message.Content = new FormUrlEncodedContent(postData);
        message.Headers.Add("Cookie", fullCookieString);

        using var responseMessage = await _client.SendAsync(message, HttpCompletionOption.ResponseContentRead, cancellationToken);
        if (!responseMessage.IsSuccessStatusCode)
            return null;

        if (await responseMessage.Content.ReadFromJsonAsync<DoLoginResult>(cancellationToken: cancellationToken) is not { } loginResult)
            return null;

        if (responseMessage.Headers.TryGetValues("Set-Cookie", out var cookies))
            loginResult.LoginSecure = GetCookieValue(cookies.First(s => s.Contains("steamLoginSecure")));

        return loginResult;
    }

    public async Task<GetListJson> MobileConf(string query, string cookieString, CancellationToken cancellationToken)
    {
        var url = ApiEndpoints.Mobileconf + query;

        using var message = new HttpRequestMessage(HttpMethod.Get, url);
        message.Headers.Add("Cookie", cookieString);

        using var responseMessage = await _client.SendAsync(message, cancellationToken);
        if (!responseMessage.IsSuccessStatusCode)
            throw new WgTokenExpiredException();

        if (await responseMessage.Content.ReadFromJsonAsync<GetListJson>(cancellationToken: cancellationToken) is not
            { Success: true } response)
            throw new WgTokenExpiredException();

        return response;
    }

    public async Task<bool> SendMultipleConfirmations(string query, string cookieString, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, ApiEndpoints.MultipleConfirmations);
        message.Content = new StringContent(query, Encoding.UTF8, "application/x-www-form-urlencoded");
        message.Headers.Add("Cookie", cookieString);

        using var responseMessage = await _client.SendAsync(message, cancellationToken);
        if (!responseMessage.IsSuccessStatusCode)
            return false;

        var response = await responseMessage.Content.ReadFromJsonAsync<SendConfirmationResponse>(cancellationToken: cancellationToken);
        return response?.Success ?? false;
    }

    public async Task<bool> SendSingleConfirmations(string query, string cookieString, CancellationToken cancellationToken)
    {
        var url = ApiEndpoints.SingleConfirmations + query;

        using var message = new HttpRequestMessage(HttpMethod.Get, url);
        message.Headers.Add("Cookie", cookieString);

        using var responseMessage = await _client.SendAsync(message, cancellationToken);
        if (!responseMessage.IsSuccessStatusCode)
            return false;

        var response = await responseMessage.Content.ReadFromJsonAsync<SendConfirmationResponse>(cancellationToken: cancellationToken);
        return response?.Success ?? false;
    }

    private static string GetCookieValue(ReadOnlySpan<char> cookie)
    {
        var equalsIndex = cookie.IndexOf('=');
        cookie = cookie.Slice(equalsIndex + 1);

        var separationCharIndex = cookie.IndexOf(";");
        cookie = cookie.Slice(0, separationCharIndex);

        return cookie.ToString();
    }
}
