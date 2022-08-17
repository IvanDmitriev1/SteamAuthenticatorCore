using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using SteamAuthenticatorCore.Shared.Services;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public class ConfirmationsViewModel
{
    public ConfirmationsViewModel(ConfirmationServiceBase confirmationServiceBase)
    {
        ConfirmationServiceBase = confirmationServiceBase;
        CheckConfirmationsCommand = new AsyncRelayCommand(confirmationServiceBase.CheckConfirmations);
    }

    public ConfirmationServiceBase ConfirmationServiceBase { get; }
    public ICommand CheckConfirmationsCommand { get; }
}
