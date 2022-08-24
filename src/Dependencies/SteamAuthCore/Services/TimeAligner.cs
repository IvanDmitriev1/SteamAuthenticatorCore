using System;
using System.Threading.Tasks;
using SteamAuthCore.Abstractions;

namespace SteamAuthCore.Services;

internal class TimeAligner : ITimeAligner
{
    public TimeAligner(ISteamApi steamApi)
    {
        _steamApi = steamApi;
    }

    private readonly ISteamApi _steamApi;
    private long _timeDifference;

    public long SteamTime => GetSystemUnixTime() + _timeDifference;

    public async ValueTask AlignTimeAsync()
    {
        var time = await _steamApi.GetSteamTime();
        _timeDifference = long.Parse(time) - GetSystemUnixTime();
    }

    private static long GetSystemUnixTime() => Convert.ToInt64(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
}