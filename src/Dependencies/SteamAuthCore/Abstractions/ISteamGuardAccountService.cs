using System.Threading.Tasks;

namespace SteamAuthCore.Abstractions;

public interface ISteamGuardAccountService
{
    ValueTask<bool> RefreshSession(SteamGuardAccount account);
}
