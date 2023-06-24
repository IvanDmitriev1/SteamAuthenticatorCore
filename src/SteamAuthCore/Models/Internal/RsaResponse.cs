namespace SteamAuthCore.Models.Internal;

internal class RsaResponse
{
	[JsonPropertyName("success")]
	public bool Success { get; set; }

	[JsonPropertyName("publickey_exp")]
	public required string Exponent { get; set; }

	[JsonPropertyName("publickey_mod")]
	public required string Modulus { get; set; }

	[JsonPropertyName("timestamp")]
	public required string Timestamp { get; set; }

	[JsonPropertyName("steamid")]
	public ulong SteamId { get; set; }

}