using System.Text.Json.Serialization;

namespace SteamAuthenticatorCore.Shared.Models.GithubApi;

class Asset
{
    [JsonPropertyName("browser_download_url")]
    public string BrowserDownloadUrl { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}