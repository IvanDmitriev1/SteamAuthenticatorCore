using System;
using System.Threading.Tasks;

namespace SteamAuthCore.Abstractions;

/// <summary>
/// Class to help align system time with the Steam server time. Not super advanced; probably not taking some things into account that it should.
/// Necessary to generate up-to-date codes. In general, this will have an error of less than a second, assuming Steam is operational.
/// </summary>
public interface ITimeAligner
{
    public Int64 SteamTime { get; }

    ValueTask AlignTimeAsync();
}
