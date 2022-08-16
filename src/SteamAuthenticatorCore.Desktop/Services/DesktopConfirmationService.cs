using System.Collections.ObjectModel;
using SteamAuthCore;
using SteamAuthenticatorCore.Desktop.Models;
using SteamAuthenticatorCore.Shared;
using SteamAuthenticatorCore.Shared.Abstraction;
using SteamAuthenticatorCore.Shared.Models;
using SteamAuthenticatorCore.Shared.Services;

namespace SteamAuthenticatorCore.Desktop.Services;

internal class DesktopConfirmationService : ConfirmationServiceBase
{
    public DesktopConfirmationService(ObservableCollection<SteamGuardAccount> steamGuardAccounts, AppSettings settings, IPlatformImplementations platformImplementations, IPlatformTimer timer) : base(steamGuardAccounts, settings, platformImplementations, timer)
    {
    }

    protected override ConfirmationAccountModelBase CreateConfirmationAccountViewModel(SteamGuardAccount account, ConfirmationModel[] confirmation, IPlatformImplementations platformImplementations) => new ConfirmationAccountModel(account, confirmation, platformImplementations);
}