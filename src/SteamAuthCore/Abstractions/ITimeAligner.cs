namespace SteamAuthCore.Abstractions;

/// <summary>
/// Class to help align system time with the Steam server time. Not super advanced; probably not taking some things into account that it should.
/// Necessary to generate up-to-date codes. In general, this will have an error of less than a second, assuming Steam is operational.
/// </summary>
public interface ITimeAligner
{
    public static Int64 SteamTime => GetSystemUnixTime() + TimeDifference;
    internal static Int64 TimeDifference;

    ValueTask AlignTimeAsync();

    internal static Int64 GetSystemUnixTime() => Convert.ToInt64(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
}
