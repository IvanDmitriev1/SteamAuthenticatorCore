namespace SteamAuthCore.Models.Internal;

internal class Finalizelogin
{
	[JsonPropertyName("steamID")]
	public string? SteamId { get; set; }

	[JsonPropertyName("redir")]
	public string? Redir { get; set; }

	[JsonPropertyName("transfer_info")]
	public List<TransferInfoData>? TransferInfo { get; set; }

	[JsonPropertyName("primary_domain")]
	public required string PrimaryDomain { get; set; }

	public class TransferInfoData
	{
		[JsonPropertyName("url")]
		public string? Url { get; set; }

		[JsonPropertyName("params")]
		public required ParamsData Params { get; set; }
	}

	public class ParamsData
	{
		[JsonPropertyName("nonce")]
		public required string Nonce { get; set; }

		[JsonPropertyName("auth")]
		public required string Auth { get; set; }
	}
}