using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using SteamAuthenticatorCore.Shared.Abstractions;
using Wpf.Ui.Mvvm.Contracts;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public partial class ConfirmationsOverviewViewModel
{
    public ConfirmationsOverviewViewModel(IConfirmationService confirmationServiceBase, INavigationService navigationService)
    {
        _navigationService = navigationService;
        ConfirmationServiceBase = confirmationServiceBase;

        CheckConfirmationsCommand = new AsyncRelayCommand(CheckConfirmations);
    }

    private readonly INavigationService _navigationService;

    public IConfirmationService ConfirmationServiceBase { get; }
    public ICommand CheckConfirmationsCommand { get; }

    private async Task CheckConfirmations()
    {
        await ConfirmationServiceBase.CheckConfirmations();
    }

    [RelayCommand]
    private void OnClick(IConfirmationViewModel viewModel)
    {
        _navigationService.NavigateTo("/accountConfirmations", viewModel);
    }
}
