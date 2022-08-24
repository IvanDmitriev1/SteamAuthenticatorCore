using System.Threading.Tasks;
using SteamAuthCore;

namespace SteamAuthenticatorCore.Shared.Abstractions;

public interface ILoginService
{
    Task RefreshLogin(SteamGuardAccount account, string password);
}
