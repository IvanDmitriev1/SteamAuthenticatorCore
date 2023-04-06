namespace SteamAuthenticatorCore.Desktop.ViewModels;

public partial class ConfirmationsOverviewViewModel : ObservableObject
{
    public ConfirmationsOverviewViewModel(IConfirmationService confirmationServiceBase, IMessenger messenger)
    {
        _messenger = messenger;
        _confirmationServiceBase = confirmationServiceBase;
    }

    private readonly IMessenger _messenger;
    private readonly IConfirmationService _confirmationServiceBase;

    [ObservableProperty]
    private ObservableCollection<SteamGuardAccountConfirmationsModel> _confirmations = new();

    [RelayCommand]
    private async Task CheckConfirmations()
    {
        var confirmations = await _confirmationServiceBase.CheckConfirmationFromAllAccounts();
        Confirmations = new ObservableCollection<SteamGuardAccountConfirmationsModel>(confirmations);
    }

    [RelayCommand]
    private void OnClick(SteamGuardAccountConfirmationsModel viewModel)
    {
        NavigationService.Default.NavigateWithHierarchy(typeof(AccountConfirmations));
        _messenger.Send(new UpdateAccountConfirmationPageMessage(viewModel));
    }
}
