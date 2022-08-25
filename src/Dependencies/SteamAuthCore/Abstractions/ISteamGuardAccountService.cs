using System.Collections.Generic;
using System.Threading.Tasks;
using SteamAuthCore.Models;

namespace SteamAuthCore.Abstractions;

public interface ISteamGuardAccountService
{
    ValueTask<bool> RefreshSession(SteamGuardAccount account);
    ValueTask<IEnumerable<ConfirmationModel>> FetchConfirmations(SteamGuardAccount account);
    ValueTask<bool> SendConfirmation(SteamGuardAccount account, ConfirmationModel confirmation, ConfirmationOptions options);
    ValueTask<bool> SendConfirmation(SteamGuardAccount account, IEnumerable<ConfirmationModel> confirmations, ConfirmationOptions options);
    Task<LoginResult> Login(LoginData loginData);
}
