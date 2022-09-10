using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using SteamAuthenticatorCore.Shared.Abstractions;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public class ConfirmationsOverviewViewModel
{
    public ConfirmationsOverviewViewModel(IConfirmationService confirmationServiceBase)
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
