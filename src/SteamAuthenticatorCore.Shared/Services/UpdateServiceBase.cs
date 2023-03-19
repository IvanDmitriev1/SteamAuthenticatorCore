using System;
using System.Threading.Tasks;
using SteamAuthenticatorCore.Shared.Abstractions;
using Octokit;

namespace SteamAuthenticatorCore.Shared.Services;

public abstract class UpdateServiceBase : IUpdateService
{
    protected UpdateServiceBase(Version currentVersion)
    {
        CurrentVersion = currentVersion;
        Client = new GitHubClient(new ProductHeaderValue("User"));
    }

    protected readonly GitHubClient Client;
    protected readonly Version CurrentVersion;

    public async Task<Release?> CheckForUpdate()
    {
        var gitHubClient = new GitHubClient(new ProductHeaderValue("User"));
        var release = await gitHubClient.Repository.Release.GetLatest("IvanDmitriev1", "SteamAuthenticatorCore");

        var version = release.TagName.TrimStart('v');
        var newVersion = new Version(version);

        return newVersion.CompareTo(CurrentVersion) == 0 ? release : null;
    }

    public abstract Task DownloadAndInstall(Release release);
}