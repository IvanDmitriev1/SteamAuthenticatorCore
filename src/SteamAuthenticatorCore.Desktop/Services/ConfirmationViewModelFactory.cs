using System.Collections.Generic;
using SteamAuthCore;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Desktop.Models;
using SteamAuthenticatorCore.Shared.Abstraction;

namespace SteamAuthenticatorCore.Desktop.Services;

internal class ConfirmationViewModelFactory : IConfirmationViewModelFactory
{
    public ConfirmationViewModelFactory(ISteamGuardAccountService accountService, IPlatformImplementations platformImplementations)
    {
        _accountService = accountService;
        _platformImplementations = platformImplementations;
    }

    private readonly ISteamGuardAccountService _accountService;
    private readonly IPlatformImplementations _platformImplementations;

    public IConfirmationViewModel Create(SteamGuardAccount account, IEnumerable<ConfirmationModel> confirmationModels) => new ConfirmationViewModel(account, confirmationModels, _platformImplementations, _accountService);
}
