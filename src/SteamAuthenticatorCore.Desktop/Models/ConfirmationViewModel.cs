using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.Models;

namespace SteamAuthenticatorCore.Desktop.Models;

public sealed class ConfirmationViewModel : ConfirmationViewModelBase
{
    public ConfirmationViewModel(SteamGuardAccount account, IEnumerable<ConfirmationModel> confirmations, IPlatformImplementations platformImplementations, ISteamGuardAccountService accountService) : base(account, confirmations, platformImplementations, accountService)
    {
        ConfirmCommand = new AsyncRelayCommand<IList>(Confirm!);
        CancelCommand = new AsyncRelayCommand<IList>(Cancel!);
    }

    public override ICommand ConfirmCommand { get; }
    public override ICommand CancelCommand { get; }

    private Task Confirm(IList list)
    {
        var confirmations = list.OfType<ConfirmationModel>();
        return SendConfirmations(confirmations, ConfirmationOptions.Allow);
    }

    private Task Cancel(IList list)
    {
        var confirmations = list.OfType<ConfirmationModel>();
        return SendConfirmations(confirmations, ConfirmationOptions.Deny);
    }
}
