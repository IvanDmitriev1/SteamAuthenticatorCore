using SQLite;

namespace SteamAuthenticatorCore.Mobile.Data;

[Table("SteamGuardAccountDto")]
internal sealed class SteamGuardAccountDto
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string? SharedSecret { get; set; }

    [NotNull]
    public string SerialNumber { get; set; } = null!;

    [NotNull]
    public string Uri { get; set; } = null!;

    [NotNull]
    public string RevocationCode { get; set; } = null!;

    [NotNull]
    public string AccountName { get; set; } = null!;

    [NotNull]
    public string IdentitySecret { get; set; } = null!;

    [NotNull]
    public string Secret1 { get; set; } = null!;

    [NotNull]
    public string DeviceId { get; set; } = null!;

    [Indexed]
    public int SessionId { get; set; }
}