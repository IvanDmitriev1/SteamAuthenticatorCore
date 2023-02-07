using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.ViewModel;
using Wpf.Ui.Contracts;
using ConfirmationModel = SteamAuthCore.Models.ConfirmationModel;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public sealed partial class ConfirmationsViewModel : ConfirmationsViewModelBase
{
    public ConfirmationsViewModel(ISteamGuardAccountService accountService, IPlatformImplementations platformImplementations, IMessenger messenger, INavigationService navigationService) : base(accountService, platformImplementations, messenger)
    {
        _navigationService = navigationService;
    }

    private readonly INavigationService _navigationService;

    [RelayCommand]
    private async Task Confirm(IList list)
    {
        var confirmations = list.OfType<ConfirmationModel>();
        await SendConfirmations(confirmations, ConfirmationOptions.Allow);

        if (ConfirmationModel.Confirmations.Count == 0)
            _navigationService.GoBack();
    }

    [RelayCommand]
    private async Task Cancel(IList list)
    {
        var confirmations = list.OfType<ConfirmationModel>();
        await SendConfirmations(confirmations, ConfirmationOptions.Deny);

        if (ConfirmationModel.Confirmations.Count == 0)
            _navigationService.GoBack();
    }
}
