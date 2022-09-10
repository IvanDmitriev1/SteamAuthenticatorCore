using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.ViewModel;
using ConfirmationModel = SteamAuthCore.Models.ConfirmationModel;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public sealed partial class ConfirmationsViewModel : ConfirmationsViewModelBase
{
    public ConfirmationsViewModel(ISteamGuardAccountService accountService, IPlatformImplementations platformImplementations, IMessenger messenger) : base(accountService, platformImplementations, messenger)
    {
    }

    [RelayCommand]
    private Task Confirm(IList list)
    {
        var confirmations = list.OfType<ConfirmationModel>();
        return SendConfirmations(confirmations, ConfirmationOptions.Allow);
    }

    [RelayCommand]
    private Task Cancel(IList list)
    {
        var confirmations = list.OfType<ConfirmationModel>();
        return SendConfirmations(confirmations, ConfirmationOptions.Deny);
    }
}
