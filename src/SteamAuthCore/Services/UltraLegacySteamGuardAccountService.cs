using SteamAuthCore.Obsolete;

namespace SteamAuthCore.Services;

internal class UltraLegacySteamGuardAccountService : ISteamGuardAccountService
{
    public UltraLegacySteamGuardAccountService(LegacySteamGuardAccountService steamGuardAccountService)
    {
        _steamGuardAccountService = steamGuardAccountService;
    }

    private readonly LegacySteamGuardAccountService _steamGuardAccountService;

    public async Task<IReadOnlyList<Confirmation>> FetchConfirmations(SteamGuardAccount account, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(account.DeviceId))
            throw new ArgumentException("Device ID is not present");

        var builder = new StringBuilder(140 + 5);
        builder.Append(ApiEndpoints.CommunityBase + ApiEndpoints.Mobileconf);
        builder.Append("conf?");
        builder.Append(LegacySteamGuardAccountService.GenerateConfirmationQueryParams(account, "conf"));

        if (await SteamApi.RequestAsync<GetListJson>(builder.ToString(), SteamApi.RequestMethod.Get, "", account.Session.GetCookieContainer()) is not { } response)
            throw new WgTokenExpiredException();

        return response.Conf ?? new List<Confirmation>();
    }

    public Task<bool> SendConfirmation(SteamGuardAccount account, Confirmation confirmation, ConfirmationOptions options, CancellationToken cancellationToken)
    {
        return _steamGuardAccountService.SendConfirmation(account, confirmation, options, cancellationToken);
    }

    public Task<bool> SendConfirmation(SteamGuardAccount account, IReadOnlyList<Confirmation> confirmations, ConfirmationOptions options, CancellationToken cancellationToken)
    {
        return _steamGuardAccountService.SendConfirmation(account, confirmations, options, cancellationToken);
    }
}