using System.Collections.Generic;
using SteamAuthCore;
using SteamAuthCore.Models;

namespace SteamAuthenticatorCore.Shared.Abstraction;

public interface IConfirmationViewModelFactory
{
    IConfirmationViewModel Create(SteamGuardAccount account, IEnumerable<ConfirmationModel> confirmationModels);
}
