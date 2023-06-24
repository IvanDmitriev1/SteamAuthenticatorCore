namespace SteamAuthCore.Abstractions;

internal interface ILegacySteamCommunityApi
{
    ValueTask<GetListJson> MobileConf(string query, string cookieString, CancellationToken cancellationToken);
    ValueTask<SendConfirmationResponse> SendMultipleConfirmations(string query, string cookieString, CancellationToken cancellationToken);
}
