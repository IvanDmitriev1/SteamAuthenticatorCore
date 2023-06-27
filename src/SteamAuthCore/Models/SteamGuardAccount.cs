namespace SteamAuthCore.Models;

public class SteamGuardAccount : IEquatable<SteamGuardAccount>
{
    #region Properties

    [JsonPropertyName("shared_secret")]
    public string? SharedSecret { get; init; }

    [JsonPropertyName("serial_number")]
    public required string SerialNumber { get; init; }

    [JsonPropertyName("uri")]
    public required string Uri { get; init; }

    [JsonPropertyName("revocation_code")]
    public required string RevocationCode { get; init; }

    [JsonPropertyName("account_name")]
    public required string AccountName { get; init; }

    [JsonPropertyName("identity_secret")]
    public required string IdentitySecret { get; init; }

    [JsonPropertyName("secret_1")]
    public required string Secret1 { get; init; }

    [JsonPropertyName("device_id")]
    public required string DeviceId { get; init; }

    public SessionData Session
    {
        get => _sessionData ?? throw new WgTokenExpiredException();
        set => _sessionData = value;
    }

    private SessionData? _sessionData;

    #endregion

    private static readonly byte[] SteamGuardCodeTranslations = { 50, 51, 52, 53, 54, 55, 56, 57, 66, 67, 68, 70, 71, 72, 74, 75, 77, 78, 80, 81, 82, 84, 86, 87, 88, 89 };
    private byte[]? _key;

    public string GenerateSteamGuardCode()
    {
        if (string.IsNullOrEmpty(SharedSecret))
            return string.Empty;

        _key ??= Convert.FromBase64String(Regex.Unescape(SharedSecret!));
        Span<byte> timeArray = stackalloc byte[8];

        var time = ITimeAligner.SteamTime;

        time /= 30L;

        for (int i = 8; i > 0; i--)
        {
            timeArray[i - 1] = (byte)time;
            time >>= 8;
        }

        Span<byte> hashedSpan = stackalloc byte[20];
        HMACSHA1.HashData(_key, timeArray, hashedSpan);

        Span<byte> codeArray = stackalloc byte[5];
        try
        {
            byte b = (byte)(hashedSpan[19] & 0xF);
            int codePoint = (hashedSpan[b] & 0x7F) << 24 | (hashedSpan[b + 1] & 0xFF) << 16 |
                            (hashedSpan[b + 2] & 0xFF) << 8 | (hashedSpan[b + 3] & 0xFF);

            for (int i = 0; i < 5; ++i)
            {
                codeArray[i] = SteamGuardCodeTranslations[codePoint % SteamGuardCodeTranslations.Length];
                codePoint /= SteamGuardCodeTranslations.Length;
            }
        }
        catch (Exception)
        {
            return string.Empty; //Change later, catch-alls are bad!
        }

        return Encoding.UTF8.GetString(codeArray);
    }

    public bool Equals(SteamGuardAccount? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other)) 
            return true;

        return AccountName == other.AccountName &&
               IdentitySecret == other.IdentitySecret &&
               Secret1 == other.Secret1;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj.GetType() != this.GetType())
            return false;

        return Equals((SteamGuardAccount)obj);
    }

    public override int GetHashCode() => HashCode.Combine(RevocationCode, AccountName, IdentitySecret, Secret1);

    public static bool operator ==(SteamGuardAccount first, SteamGuardAccount second) => first.Equals(second);
    public static bool operator !=(SteamGuardAccount first, SteamGuardAccount second) => !(first == second);
}