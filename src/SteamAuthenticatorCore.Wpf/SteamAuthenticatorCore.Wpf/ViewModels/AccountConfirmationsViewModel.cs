namespace SteamAuthenticatorCore.Desktop.ViewModels;

public sealed partial class AccountConfirmationsViewModel : BaseAccountConfirmationsViewModel
{
    public AccountConfirmationsViewModel(ISteamGuardAccountService accountService, IPlatformImplementations platformImplementations) : base(accountService, platformImplementations)
    {
        
    }

    [RelayCommand]
    private async Task Confirm(IList list)
    {
        if (Model is null)
            return;

        var confirmations = list.OfType<ConfirmationModel>();
        await SendConfirmations(confirmations, ConfirmationOptions.Allow);

        if (Model.Confirmations.Count == 0)
            NavigationService.Default.GoBack();
    }

    [RelayCommand]
    private async Task Cancel(IList list)
    {
        if (Model is null)
            return;

        var confirmations = list.OfType<ConfirmationModel>();
        await SendConfirmations(confirmations, ConfirmationOptions.Deny);

        if (Model.Confirmations.Count == 0)
            NavigationService.Default.GoBack();
    }
}
