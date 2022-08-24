using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using SteamAuthCore;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Shared.Abstraction;
using SteamAuthenticatorCore.Shared.Models;

namespace SteamAuthenticatorCore.Desktop.Models;

public sealed class ConfirmationAccountModel : ConfirmationAccountModelBase
{
    public ConfirmationAccountModel(SteamGuardAccount account, ConfirmationModel[] confirmations, IPlatformImplementations platformImplementations) : base(account, confirmations, platformImplementations)
    {
        ConfirmCommand = new AsyncRelayCommand<IList>(Confirm!);
        CancelCommand = new AsyncRelayCommand<IList>(Cancel!);
    }

    public override ICommand ConfirmCommand { get; }
    public override ICommand CancelCommand { get; }

    private Task Confirm(IList list)
    {
        var confirmations = list.OfType<ConfirmationModel>();
        return SendConfirmations(confirmations, ConfirmationOptions.Deny);
    }

    private Task Cancel(IList list)
    {
        var confirmations = list.OfType<ConfirmationModel>();
        return SendConfirmations(confirmations, ConfirmationOptions.Deny);
    }
}
