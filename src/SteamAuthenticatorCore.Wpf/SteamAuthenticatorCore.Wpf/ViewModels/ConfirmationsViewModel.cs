using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Desktop.Services;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.ViewModel;
using Wpf.Ui.Contracts;
using ConfirmationModel = SteamAuthCore.Models.ConfirmationModel;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public sealed partial class ConfirmationsViewModel : ConfirmationsViewModelBase
{
    public ConfirmationsViewModel(ISteamGuardAccountService accountService, IPlatformImplementations platformImplementations, IMessenger messenger) : base(accountService, platformImplementations, messenger)
    {
        
    }

    [RelayCommand]
    private async Task Confirm(IList list)
    {
        var confirmations = list.OfType<ConfirmationModel>();
        await SendConfirmations(confirmations, ConfirmationOptions.Allow);

        if (SteamGuardAccountConfirmationsModel.Confirmations.Count == 0)
            NavigationService.Default.GoBack();
    }

    [RelayCommand]
    private async Task Cancel(IList list)
    {
        var confirmations = list.OfType<ConfirmationModel>();
        await SendConfirmations(confirmations, ConfirmationOptions.Deny);

        if (SteamGuardAccountConfirmationsModel.Confirmations.Count == 0)
            NavigationService.Default.GoBack();
    }
}
