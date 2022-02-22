using System.Windows.Input;
using SteamAuthenticatorCore.Shared;
using WpfHelper.Commands;

namespace SteamAuthenticatorCore.Desktop.ViewModels
{
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
}
