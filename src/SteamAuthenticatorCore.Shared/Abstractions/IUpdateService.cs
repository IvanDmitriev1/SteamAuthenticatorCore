using Octokit;

namespace SteamAuthenticatorCore.Shared.Abstractions;

public interface IUpdateService
{
    Task<Release?> CheckForUpdate();
}
