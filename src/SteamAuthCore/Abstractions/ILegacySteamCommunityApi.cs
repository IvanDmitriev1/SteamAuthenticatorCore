namespace SteamAuthCore.Abstractions;

internal interface ILegacySteamCommunityApi
{
    Task<GetListJson> MobileConf(string query, string cookieString, CancellationToken cancellationToken);
    Task<bool> SendMultipleConfirmations(string query, string cookieString, CancellationToken cancellationToken);
    Task<bool> SendSingleConfirmations(string query, string cookieString, CancellationToken cancellationToken);
}
