using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using SteamAuthenticatorCore.Shared.Abstraction;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public class ConfirmationsViewModel
{
    public ConfirmationsViewModel(IConfirmationService confirmationServiceBase)
    {
        ConfirmationServiceBase = confirmationServiceBase;

        CheckConfirmationsCommand = new AsyncRelayCommand(CheckConfirmations);
    }

    public IConfirmationService ConfirmationServiceBase { get; }
    public ICommand CheckConfirmationsCommand { get; }

    private async Task CheckConfirmations()
    {
        await ConfirmationServiceBase.CheckConfirmations();
    }
}
