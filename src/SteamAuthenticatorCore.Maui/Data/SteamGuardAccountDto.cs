using SQLite;

namespace SteamAuthenticatorCore.Maui.Data;

[Table("SteamGuardAccountDto")]
internal sealed class SteamGuardAccountDto : IEquatable<SteamGuardAccount>
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string? SharedSecret { get; set; }

    [NotNull]
    public string SerialNumber { get; init; } = null!;

    [NotNull]
    public string Uri { get; init; } = null!;

    [NotNull]
    public string RevocationCode { get; init; } = null!;

    [NotNull]
    public string AccountName { get; init; } = null!;

    [NotNull]
    public string IdentitySecret { get; init; } = null!;

    [NotNull]
    public string Secret1 { get; init; } = null!;

    [NotNull]
    public string DeviceId { get; init; } = null!;

    [Indexed]
    public int SessionId { get; set; }

    public bool Equals(SteamGuardAccount? other)
    {
        if (other is null)
            return false;

        return SerialNumber == other.SerialNumber && RevocationCode == other.RevocationCode && AccountName == other.AccountName && IdentitySecret == other.IdentitySecret && Secret1 == other.Secret1;
    }

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is SteamGuardAccountDto other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(SerialNumber, RevocationCode, AccountName, IdentitySecret, Secret1);

    public static bool operator ==(SteamGuardAccountDto dto, SteamGuardAccount account) => dto.Equals(account);
    public static bool operator !=(SteamGuardAccountDto dto, SteamGuardAccount account) => !(dto == account);
}