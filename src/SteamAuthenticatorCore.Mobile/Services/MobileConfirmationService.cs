using System.Collections.ObjectModel;
using SteamAuthCore;
using SteamAuthenticatorCore.Mobile.Models;
using SteamAuthenticatorCore.Shared;
using SteamAuthenticatorCore.Shared.Abstraction;
using SteamAuthenticatorCore.Shared.Models;
using SteamAuthenticatorCore.Shared.Services;

namespace SteamAuthenticatorCore.Mobile.Services;

internal class MobileConfirmationService : ConfirmationServiceBase
{
    public MobileConfirmationService(ObservableCollection<SteamGuardAccount> steamGuardAccounts, AppSettings settings, IPlatformImplementations platformImplementations, IPlatformTimer timer) : base(steamGuardAccounts, settings, platformImplementations, timer)
    {
    }

    protected override ConfirmationAccountModelBase CreateConfirmationAccountViewModel(SteamGuardAccount account,
        ConfirmationModel[] confirmation, IPlatformImplementations platformImplementations) =>
        new ConfirmationAccountModel(account, confirmation, platformImplementations);
}