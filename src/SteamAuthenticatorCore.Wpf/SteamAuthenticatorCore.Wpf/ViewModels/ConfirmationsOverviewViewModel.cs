using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.Messages;
using SteamAuthenticatorCore.Shared.Models;
using Wpf.Ui.Contracts;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public partial class ConfirmationsOverviewViewModel
{
    public ConfirmationsOverviewViewModel(IConfirmationService confirmationServiceBase, INavigationService navigationService, IMessenger messenger)
    {
        _navigationService = navigationService;
        _messenger = messenger;
        ConfirmationServiceBase = confirmationServiceBase;
    }

    private readonly INavigationService _navigationService;
    private readonly IMessenger _messenger;

    public IConfirmationService ConfirmationServiceBase { get; }

    [RelayCommand]
    private Task CheckConfirmations() => ConfirmationServiceBase.CheckConfirmations();

    [RelayCommand]
    private void OnClick(ConfirmationModel viewModel)
    {
        //TODO

        //_navigationService.Navigate("/accountConfirmations");
        //_messenger.Send(new UpdateAccountConfirmationPageMessage(viewModel));
    }
}
