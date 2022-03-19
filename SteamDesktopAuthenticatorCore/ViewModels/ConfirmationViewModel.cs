using System.Windows.Input;
using SteamAuthenticatorCore.Desktop.Helpers;
using SteamAuthenticatorCore.Shared;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public class ConfirmationViewModel
{
    public ConfirmationViewModel(BaseConfirmationService confirmationService)
    {
        ConfirmationService = confirmationService;

        CheckConfirmationsCommand = new AsyncRelayCommand(async () =>
        {
            await ConfirmationService.CheckConfirmations();
        });
    }

    public BaseConfirmationService ConfirmationService { get; }

    public ICommand CheckConfirmationsCommand { get; }
}