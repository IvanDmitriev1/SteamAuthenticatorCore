using Octokit;
using ProductHeaderValue = Octokit.ProductHeaderValue;

namespace SteamAuthenticatorCore.Shared.Services;

public sealed class UpdateService : IUpdateService
{
    public UpdateService(Version currentVersion)
    {
        _currentVersion = currentVersion;
        _githubClient = new GitHubClient(new ProductHeaderValue("User"));
    }

    private readonly GitHubClient _githubClient;
    private readonly Version _currentVersion;

    public async Task<Release?> CheckForUpdate()
    {
        var release = await _githubClient.Repository.Release.GetLatest("IvanDmitriev1", "SteamAuthenticatorCore");

        var version = release.TagName.TrimStart('v');
        var newVersion = new Version(version);

        return newVersion.CompareTo(_currentVersion) == 0 ? release : null;
    }
}