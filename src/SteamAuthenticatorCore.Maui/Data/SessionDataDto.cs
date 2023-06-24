using SQLite;

namespace SteamAuthenticatorCore.Maui.Data;

[Table("SessionDataDto")]
internal class SessionDataDto
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string? SessionId { get; set; }
    public string? SteamLoginSecure { get; set; }
    public string? AccessToken { get; set; }
    public string? WebCookie { get; set; }
    public Int64 SteamId { get; set; }
}