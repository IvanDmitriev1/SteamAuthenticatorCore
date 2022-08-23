using SteamAuthCore;
using System.Threading.Tasks;

namespace SteamAuthenticatorCore.Shared.Abstraction;

public interface ILoginService
{
    Task RefreshLogin(SteamGuardAccount account, string password);
}
