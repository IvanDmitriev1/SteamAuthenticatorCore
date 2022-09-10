using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.Messages;
using SteamAuthenticatorCore.Shared.Models;
using Wpf.Ui.Mvvm.Contracts;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public partial class ConfirmationsOverviewViewModel
{
    public ConfirmationsOverviewViewModel(IConfirmationService confirmationServiceBase, INavigationService navigationService, IMessenger messenger)
    {
        _navigationService = navigationService;
        _messenger = messenger;
        ConfirmationServiceBase = confirmationServiceBase;

        CheckConfirmationsCommand = new AsyncRelayCommand(CheckConfirmations);
    }

    private readonly INavigationService _navigationService;
    private readonly IMessenger _messenger;

    public IConfirmationService ConfirmationServiceBase { get; }
    public ICommand CheckConfirmationsCommand { get; }

    private async Task CheckConfirmations()
    {
        await ConfirmationServiceBase.CheckConfirmations();
    }

    [RelayCommand]
    private void OnClick(ConfirmationModel viewModel)
    {
        _navigationService.NavigateTo("/accountConfirmations");
        _messenger.Send(new UpdateAccountConfirmationPageMessage(viewModel));
    }
}
