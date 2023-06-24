namespace SteamAuthCore.Models.Internal;

internal class GetListJson
{
	[JsonPropertyName("success")]
	public bool Success { get; set; }

	[JsonPropertyName("needauth")]
	public bool Needauth { get; set; }

	[JsonPropertyName("conf")]
	public List<Confirmation>? Conf { get; set; }
}