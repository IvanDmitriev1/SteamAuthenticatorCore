using System.Threading.Tasks;
using System.Windows.Input;
using SteamAuthCore;
using SteamAuthenticatorCore.Shared.Abstraction;
using SteamAuthenticatorCore.Shared.Models;
using Xamarin.CommunityToolkit.ObjectModel;

namespace SteamAuthenticatorCore.Mobile.Models;

public sealed class ConfirmationAccountModel : ConfirmationAccountModelBase
{
    public ConfirmationAccountModel(SteamGuardAccount account, ConfirmationModel[] confirmations, IPlatformImplementations platformImplementations) : base(account, confirmations, platformImplementations)
    {
        ConfirmCommand = new AsyncCommand<ConfirmationModel>(Confirm!);
        CancelCommand = new AsyncCommand<ConfirmationModel>(Cancel!);
    }

    public override ICommand ConfirmCommand { get; }

    public override ICommand CancelCommand { get; }

    private Task Confirm(ConfirmationModel confirmation)
    {
        return SendConfirmation(confirmation, SteamGuardAccount.Confirmation.Allow);
    }

    private Task Cancel(ConfirmationModel confirmation)
    {
        return SendConfirmation(confirmation, SteamGuardAccount.Confirmation.Deny);
    }
}