using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net.Http.Json;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.Models.GithubApi;
using SteamAuthenticatorCore.Shared.Models;

namespace SteamAuthenticatorCore.Shared.Services;

public abstract class UpdateServiceBase : IUpdateService
{
    protected UpdateServiceBase(HttpClient client)
    {
        Client = client;
        Client.DefaultRequestHeaders.Add("User-Agent", "User");
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        _repo = "IvanDmitriev1/SteamAuthenticatorCore";
    }

    private readonly string _repo;
    protected readonly HttpClient Client;

    public async ValueTask<CheckForUpdateModel?> CheckForUpdate(string fileContains, Version currentVersion)
    {
        if (await Client.GetFromJsonAsync<GitHubRequestApiModel>($"https://api.github.com/repos/{_repo}/releases/latest").ConfigureAwait(false) is not { } apiModel)
            return null;

        var downloadUrl = string.Empty;
        foreach (var asset in apiModel.Assets)
        {
            if (!asset.Name.Contains(fileContains))
                continue;

            downloadUrl = asset.BrowserDownloadUrl;
            break;
        }

        Version newVersion = new(apiModel.TagName);

        switch (newVersion.CompareTo(currentVersion))
        {
            case 0:
            case < 0:
                return new CheckForUpdateModel(downloadUrl, newVersion, false);
            case > 0:
                return new CheckForUpdateModel(downloadUrl, newVersion, true);
        }
    }

    public abstract ValueTask CheckForUpdateAndDownloadInstall(bool isInBackground);
    public abstract Task DownloadAndInstall(CheckForUpdateModel updateModel);
}