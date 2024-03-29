﻿namespace SteamAuthCore.Abstractions;

internal interface ILegacySteamCommunityApi
{
    Task<string?> GenerateSessionIdCookieForLogin(CancellationToken cancellationToken);
    Task<RsaResponse?> LoginGetRsaKey(string userName, CancellationToken cancellationToken);
    Task<DoLoginResult?> DoLogin(KeyValuePair<string, string>[] postData, string cookieString, CancellationToken cancellationToken);

    Task<GetListJson> MobileConf(string query, string cookieString, CancellationToken cancellationToken);
    Task<bool> SendMultipleConfirmations(IReadOnlyList<KeyValuePair<string, string>> postData, string cookieString, CancellationToken cancellationToken);
    Task<bool> SendSingleConfirmations(string query, string cookieString, CancellationToken cancellationToken);
}
