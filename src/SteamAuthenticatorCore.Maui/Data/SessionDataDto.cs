using SQLite;

namespace SteamAuthenticatorCore.Maui.Data;

[Table("SessionDataDto")]
internal class SessionDataDto
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull]
    public string SessionId { get; set; } = null!;

    [NotNull]
    public string SteamLogin { get; set; } = null!;

    [NotNull]
    public string SteamLoginSecure { get; set; } = null!;

    [NotNull]
    public string WebCookie { get; set; } = null!;

    [NotNull]
    public string OAuthToken { get; set; } = null!;

    public Int64 SteamId { get; set; }
}