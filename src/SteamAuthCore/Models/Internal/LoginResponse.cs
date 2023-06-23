namespace SteamAuthCore.Models.Internal;

internal class LoginResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("requires_twofactor")]
    public bool TwoFactorNeeded { get; set; }

    [JsonPropertyName("login_complete")]
    public bool LoginComplete { get; set; }

    [JsonPropertyName("emailauth_needed")]
    public bool EmailAuthNeeded { get; set; }

    [JsonPropertyName("emailsteamid")]
    public ulong EmailSteamId { get; set; }


    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("clear_password_field")]
    public bool ClearPasswordField { get; set; }

    [JsonPropertyName("captcha_needed")]
    public bool CaptchaNeeded { get; set; }

    [JsonPropertyName("captcha_gid")]
    public string? CaptchaGid { get; set; }

    [JsonPropertyName("transfer_parameters")]
    public TransferParameters? TransferParameters { get; set; }
}
