using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using SteamAuthenticatorCore.Shared.Abstraction;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public class ConfirmationsViewModel
{
    public ConfirmationsViewModel(IConfirmationService confirmationServiceBase)
    {
        ConfirmationServiceBase = confirmationServiceBase;

        CheckConfirmationsCommand = new AsyncRelayCommand( async () =>
        {
            await confirmationServiceBase.CheckConfirmations();
        });
    }

    public IConfirmationService ConfirmationServiceBase { get; }
    public ICommand CheckConfirmationsCommand { get; }
}
