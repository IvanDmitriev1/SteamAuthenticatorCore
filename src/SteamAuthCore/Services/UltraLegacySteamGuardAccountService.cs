using SteamAuthCore.Obsolete;

namespace SteamAuthCore.Services;

internal class UltraLegacySteamGuardAccountService : ISteamGuardAccountService
{
    public UltraLegacySteamGuardAccountService(LegacySteamGuardAccountService steamGuardAccountService)
    {
        _steamGuardAccountService = steamGuardAccountService;
    }

    private readonly LegacySteamGuardAccountService _steamGuardAccountService;

    public async Task<IEnumerable<ConfirmationModel>> FetchConfirmations(SteamGuardAccount account, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(account.DeviceId))
            throw new ArgumentException("Device ID is not present");

        if (await SendFetchConfirmationsRequest(account, cancellationToken) is not { } html)
            return Enumerable.Empty<ConfirmationModel>();

        using var document = await LegacySteamGuardAccountService.Parser.ParseDocumentAsync(html, cancellationToken);
        return document.GetElementsByClassName("mobileconf_list_entry").Select(LegacySteamGuardAccountService.GetConfirmationModelFromHtml);
    }

    public Task<bool> SendConfirmation(SteamGuardAccount account, ConfirmationModel confirmation, ConfirmationOptions options, CancellationToken cancellationToken)
    {
        return _steamGuardAccountService.SendConfirmation(account, confirmation, options, cancellationToken);
    }

    public Task<bool> SendConfirmation(SteamGuardAccount account, ConfirmationModel[] confirmations, ConfirmationOptions options, CancellationToken cancellationToken)
    {
        return _steamGuardAccountService.SendConfirmation(account, confirmations, options, cancellationToken);
    }

    private async Task<string?> SendFetchConfirmationsRequest(SteamGuardAccount account, CancellationToken cancellationToken, UInt16 times = 0)
    {
        try
        {
            var queryBuilder = new StringBuilder(140 + 5);
            queryBuilder.Append("conf?");
            queryBuilder.Append(LegacySteamGuardAccountService.GenerateConfirmationQueryParams(account, "conf"));

            var url = ApiEndpoints.CommunityBase + ApiEndpoints.Mobileconf + queryBuilder;

            var html =
                await SteamApi.RequestAsync(url, SteamApi.RequestMethod.Get, "", account.Session.GetCookieContainer());

            if (html is null)
                throw new WgTokenInvalidException();

            if (html.Contains("Nothing to confirm"))
                return null;

            if (!html.Contains("Invalid authenticator"))
                return html;

            return await SendFetchConfirmationsRequest(account, cancellationToken);
        }
        catch (WgTokenInvalidException)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

            if (times >= 1)
                return null;

            return await SendFetchConfirmationsRequest(account, cancellationToken, ++times);
        }
        catch (WgTokenExpiredException)
        {
            return null;
        }
    }
}