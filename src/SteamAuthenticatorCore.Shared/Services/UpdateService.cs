using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net.Http.Json;
using SteamAuthenticatorCore.Shared.Models.GithubApi;
using System.Reflection;
using SteamAuthenticatorCore.Shared.Models;
using SteamAuthenticatorCore.Shared.Abstraction;

namespace SteamAuthenticatorCore.Shared.Services;

public abstract class UpdateServiceBase : IUpdateService
{
    public UpdateServiceBase(HttpClient client)
    {
        Client = client;
        Client.DefaultRequestHeaders.Add("User-Agent", "User");
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        _repo = "IvanDmitriev1/SteamAuthenticatorCore";
    }

    private readonly string _repo;
    protected readonly HttpClient Client;

    public async ValueTask<CheckForUpdateModel?> CheckForUpdate(string fileName)
    {
        if (await Client.GetFromJsonAsync<GitHubRequestApiModel>($"https://api.github.com/repos/{_repo}/releases/latest").ConfigureAwait(false) is not { } apiModel)
            return null;

        var downloadUrl = string.Empty;
        foreach (var asset in apiModel.Assets)
        {
            if (asset.Name != fileName)
                continue;

            downloadUrl = asset.BrowserDownloadUrl;
            break;
        }

        Version newVersion = new(apiModel.TagName);
        Version currentVersion = Assembly.GetExecutingAssembly().GetName().Version;

        switch (newVersion.CompareTo(currentVersion))
        {
            case 0:
            case < 0:
                return new CheckForUpdateModel(fileName, downloadUrl, newVersion, false);
            case > 0:
                return new CheckForUpdateModel(fileName, downloadUrl, newVersion, true);
        }
    }

    public abstract ValueTask CheckForUpdateAndDownloadInstall(bool isInBackground);
    public abstract Task DownloadAndInstall(CheckForUpdateModel updateModel);
}