using System;
using System.Threading.Tasks;
using SteamAuthCore.Abstractions;

namespace SteamAuthCore.Services;

internal class TimeAligner : ITimeAligner
{
    public TimeAligner(ILegacySteamApi legacySteamApi)
    {
        _legacySteamApi = legacySteamApi;
    }

    private readonly ILegacySteamApi _legacySteamApi;

    public async ValueTask AlignTimeAsync()
    {
        var time = await _legacySteamApi.GetServerTime().ConfigureAwait(false);
        ITimeAligner.TimeDifference = Int64.Parse(time) - ITimeAligner.GetSystemUnixTime();
    }
}