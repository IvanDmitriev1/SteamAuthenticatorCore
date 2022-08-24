using System.Threading.Tasks;
using SteamAuthCore.Abstractions;

namespace SteamAuthCore.Services;

internal class SteamGuardAccountService : ISteamGuardAccountService
{
    public SteamGuardAccountService(ISteamApi steamApi)
    {
        _steamApi = steamApi;
    }

    private readonly ISteamApi _steamApi;

    public async ValueTask<bool> RefreshSession(SteamGuardAccount account)
    {
        if (await _steamApi.MobileauthGetwgtoken(account.Session.OAuthToken) is not { } refreshResponse)
            return false;

        var token = account.Session.SteamId + "%7C%7C" + refreshResponse.Token;
        var tokenSecure = account.Session.SteamId + "%7C%7C" + refreshResponse.TokenSecure;

        account.Session.SteamLogin = token;
        account.Session.SteamLoginSecure = tokenSecure;
        return true;
    }
}
