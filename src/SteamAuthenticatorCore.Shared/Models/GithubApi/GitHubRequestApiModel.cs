using System.Globalization;
using System.Text.Json.Serialization;

namespace SteamAuthenticatorCore.Shared.Models.GithubApi;

class GitHubRequestApiModel
{
    [JsonConstructor]
    public GitHubRequestApiModel(string tagName, Asset[] assets, bool prerelease)
    {
        TagName = tagName.StartsWith("v", true, CultureInfo.CurrentCulture) ? tagName.Remove(0, 1) : tagName;
        Assets = assets;
        Prerelease = prerelease;
    }

    [JsonPropertyName("tag_name")]
    public string TagName { get; }
    
    [JsonPropertyName("assets")]
    public Asset[] Assets { get; }
    
    [JsonPropertyName("prerelease")]
    public bool Prerelease { get; }
}
