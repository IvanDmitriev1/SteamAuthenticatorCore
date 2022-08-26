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

    public async ValueTask AlignTimeAsync()
    {
        var time = await _steamApi.GetSteamTime().ConfigureAwait(false);
        ITimeAligner.TimeDifference = Int64.Parse(time) - ITimeAligner.GetSystemUnixTime();
    }
}