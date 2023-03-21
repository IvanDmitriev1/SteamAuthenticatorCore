using System.Collections.Generic;
using System.Threading.Tasks;
using SteamAuthenticatorCore.Shared.Models;

namespace SteamAuthenticatorCore.Shared.Abstractions;

public interface IConfirmationService
{
    Task Initialize();
    Task<IReadOnlyList<SteamGuardAccountConfirmationsModel>> CheckConfirmationFromAllAccounts();
}
