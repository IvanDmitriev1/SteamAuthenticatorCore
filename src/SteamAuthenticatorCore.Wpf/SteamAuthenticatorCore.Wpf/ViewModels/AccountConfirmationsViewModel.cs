using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Desktop.Services;
using SteamAuthenticatorCore.Shared.ViewModel;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public sealed partial class AccountConfirmationsViewModel : ConfirmationsViewModelBase
{
    public AccountConfirmationsViewModel(ISteamGuardAccountService accountService) : base(accountService)
    {
        
    }

    [RelayCommand]
    private async Task Confirm(IList list)
    {
        if (SteamGuardAccountConfirmationsModel is null)
            return;

        var confirmations = list.OfType<ConfirmationModel>();
        await SendConfirmations(confirmations, ConfirmationOptions.Allow);

        if (SteamGuardAccountConfirmationsModel.Confirmations.Count == 0)
            NavigationService.Default.GoBack();
    }

    [RelayCommand]
    private async Task Cancel(IList list)
    {
        if (SteamGuardAccountConfirmationsModel is null)
            return;

        var confirmations = list.OfType<ConfirmationModel>();
        await SendConfirmations(confirmations, ConfirmationOptions.Deny);

        if (SteamGuardAccountConfirmationsModel.Confirmations.Count == 0)
            NavigationService.Default.GoBack();
    }
}
