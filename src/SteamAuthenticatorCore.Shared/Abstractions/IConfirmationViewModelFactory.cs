using System.Collections.Generic;
using SteamAuthCore;
using SteamAuthCore.Models;

namespace SteamAuthenticatorCore.Shared.Abstractions;

public interface IConfirmationViewModelFactory
{
    IConfirmationViewModel Create(SteamGuardAccount account, IEnumerable<ConfirmationModel> confirmationModels);
}
