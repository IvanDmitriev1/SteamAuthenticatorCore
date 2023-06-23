namespace SteamAuthCore.Models.Internal;

internal class GetPasswordRsaPublicKey
{
	[JsonPropertyName("response")]
	public required ResponseData Response { get; set; }

	internal class ResponseData
	{
		[JsonPropertyName("publickey_mod")]
		public required string PublickeyMod { get; set; }

		[JsonPropertyName("publickey_exp")]
		public required string PublickeyExp { get; set; }

		[JsonPropertyName("timestamp")]
		public required string Timestamp { get; set; }
	}

}