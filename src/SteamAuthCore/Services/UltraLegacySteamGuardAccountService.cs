using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Exceptions;
using SteamAuthCore.Models;
using SteamAuthCore.Obsolete;

namespace SteamAuthCore.Services;

internal class UltraLegacySteamGuardAccountService : ISteamGuardAccountService
{
    public UltraLegacySteamGuardAccountService(LegacySteamGuardAccountService steamGuardAccountService)
    {
        _steamGuardAccountService = steamGuardAccountService;
    }

    private readonly LegacySteamGuardAccountService _steamGuardAccountService;

    public async ValueTask<bool> RefreshSession(SteamGuardAccount account, CancellationToken cancellationToken)
    {
        return await _steamGuardAccountService.RefreshSession(account, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<IEnumerable<ConfirmationModel>> FetchConfirmations(SteamGuardAccount account, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(account.DeviceId))
            throw new ArgumentException("Device ID is not present");

        if (await SendFetchConfirmationsRequest(account, cancellationToken).ConfigureAwait(false) is not { } html)
            return Enumerable.Empty<ConfirmationModel>();

        using var document = await LegacySteamGuardAccountService.Parser.ParseDocumentAsync(html, cancellationToken).ConfigureAwait(false);
        return document.GetElementsByClassName("mobileconf_list_entry").Select(LegacySteamGuardAccountService.GetConfirmationModelFromHtml);
    }

    public async ValueTask<bool> SendConfirmation(SteamGuardAccount account, ConfirmationModel confirmation, ConfirmationOptions options, CancellationToken cancellationToken)
    {
        return await _steamGuardAccountService.SendConfirmation(account, confirmation, options, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask<bool> SendConfirmation(SteamGuardAccount account, ConfirmationModel[] confirmations, ConfirmationOptions options, CancellationToken cancellationToken)
    {
        return await _steamGuardAccountService.SendConfirmation(account, confirmations, options, cancellationToken).ConfigureAwait(false);
    }

    public Task<LoginResult> Login(LoginData loginData)
    {
        return _steamGuardAccountService.Login(loginData);
    }

    public Task<bool> RemoveAuthenticator(SteamGuardAccount account)
    {
        return _steamGuardAccountService.RemoveAuthenticator(account);
    }

    private async ValueTask<string?> SendFetchConfirmationsRequest(SteamGuardAccount account, CancellationToken cancellationToken, UInt16 times = 0)
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

            return await SendFetchConfirmationsRequest(account, cancellationToken).ConfigureAwait(false);
        }
        catch (WgTokenInvalidException)
        {
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

            if (times >= 1)
                return null;

            if (!await RefreshSession(account, cancellationToken).ConfigureAwait(false))
                return null;

            return await SendFetchConfirmationsRequest(account, cancellationToken, ++times).ConfigureAwait(false);
        }
        catch (WgTokenExpiredException)
        {
            return null;
        }
    }
}