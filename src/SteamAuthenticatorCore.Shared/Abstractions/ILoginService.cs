using System.Threading.Tasks;
using SteamAuthCore;
using SteamAuthCore.Models;

namespace SteamAuthenticatorCore.Shared.Abstractions;

public interface ILoginService
{
    Task RefreshLogin(SteamGuardAccount account, string password);
}
