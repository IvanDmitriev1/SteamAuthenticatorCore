namespace SteamAuthenticatorCore.Shared.Abstractions;

public interface ILoginService
{
    Task<bool> RefreshLogin(SteamGuardAccount account, string password);
}
