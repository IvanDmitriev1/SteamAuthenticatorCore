namespace SteamAuthCore.Models.Internal;

internal class TransferParameters
{
	[JsonPropertyName("steamid")]
	public required string Steamid { get; set; }

	[JsonPropertyName("token_secure")]
	public required string TokenSecure { get; set; }

	[JsonPropertyName("auth")]
	public required string Auth { get; set; }

	[JsonPropertyName("remember_login")]
	public required bool RememberLogin { get; set; }

	[JsonPropertyName("webcookie")]
	public required string Webcookie { get; set; }
}