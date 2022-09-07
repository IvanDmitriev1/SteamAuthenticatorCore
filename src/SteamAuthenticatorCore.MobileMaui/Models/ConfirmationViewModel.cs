using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.Models;

namespace SteamAuthenticatorCore.MobileMaui.Models;

public sealed class ConfirmationViewModel : ConfirmationViewModelBase
{
    public ConfirmationViewModel(SteamGuardAccount account, IEnumerable<ConfirmationModel> confirmations,
        IPlatformImplementations platformImplementations, ISteamGuardAccountService accountService) : base(account,
        confirmations, platformImplementations, accountService)
    {
        ConfirmCommand = new RelayCommand(() =>
        {

        });

        CancelCommand = new RelayCommand(() =>
        {

        });
    }

    public override ICommand ConfirmCommand { get; }
    public override ICommand CancelCommand { get; }
}