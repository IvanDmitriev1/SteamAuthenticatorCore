using System.Threading.Tasks;
using SteamAuthCore.Models;

namespace SteamAuthenticatorCore.Shared.Abstractions;

public interface ILoginService
{
    Task<bool> RefreshLogin(SteamGuardAccount account, string password);
}
