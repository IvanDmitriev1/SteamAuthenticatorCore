using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Desktop.Services;
using SteamAuthenticatorCore.Shared.ViewModel;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public sealed partial class AccountConfirmationsViewModel : BaseAccountConfirmationsViewModel
{
    public AccountConfirmationsViewModel(ISteamGuardAccountService accountService) : base(accountService)
    {
        
    }

    [RelayCommand]
    private async Task Confirm(IList list)
    {
        if (Model is null)
            return;

        var confirmations = list.OfType<ConfirmationModel>();
        await SendConfirmations(confirmations, ConfirmationOptions.Allow);

        if (Model.Confirmations.Count == 0)
            NavigationService.Default.GoBack();
    }

    [RelayCommand]
    private async Task Cancel(IList list)
    {
        if (Model is null)
            return;

        var confirmations = list.OfType<ConfirmationModel>();
        await SendConfirmations(confirmations, ConfirmationOptions.Deny);

        if (Model.Confirmations.Count == 0)
            NavigationService.Default.GoBack();
    }
}
