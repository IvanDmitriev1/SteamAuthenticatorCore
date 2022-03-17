using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using SteamAuthenticatorCore.Shared;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public class ConfirmationViewModel
{
    public ConfirmationViewModel(BaseConfirmationService confirmationService)
    {
        ConfirmationService = confirmationService;
    }

    public BaseConfirmationService ConfirmationService { get; }

    public ICommand CheckConfirmationsCommand => new AsyncRelayCommand(async o =>
    {
        await ConfirmationService.CheckConfirmations();
    });
}