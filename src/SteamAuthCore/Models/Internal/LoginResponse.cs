namespace SteamAuthCore.Models.Internal;
internal class LoginResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("login_complete")]
    public bool LoginComplete { get; set; }

    [JsonPropertyName("oauth")]
    public string? OAuthDataString { get; set; }

    public OAuth? OAuthData => OAuthDataString != null ? JsonSerializer.Deserialize<OAuth>(OAuthDataString) : null;

    [JsonPropertyName("captcha_needed")]
    public bool CaptchaNeeded { get; set; }

    [JsonPropertyName("captcha_gid")]
    public string CaptchaGid { get; set; } = null!;

    [JsonPropertyName("emailsteamid")]
    public ulong EmailSteamID { get; set; }

    [JsonPropertyName("emailauth_needed")]
    public bool EmailAuthNeeded { get; set; }

    [JsonPropertyName("requires_twofactor")]
    public bool TwoFactorNeeded { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
