using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SteamAuthCore.Models;

namespace SteamAuthCore.Abstractions;

public interface ISteamGuardAccountService
{
    ValueTask<bool> RefreshSession(SteamGuardAccount account, CancellationToken cancellationToken);
    ValueTask<IEnumerable<ConfirmationModel>> FetchConfirmations(SteamGuardAccount account, CancellationToken cancellationToken);
    ValueTask<bool> SendConfirmation(SteamGuardAccount account, ConfirmationModel confirmation, ConfirmationOptions options, CancellationToken cancellationToken);
    ValueTask<bool> SendConfirmation(SteamGuardAccount account, IEnumerable<ConfirmationModel> confirmations, ConfirmationOptions options, CancellationToken cancellationToken);
    Task<LoginResult> Login(LoginData loginData);
    Task<bool> RemoveAuthenticator(SteamGuardAccount account);
}
