namespace SteamAuthenticatorCore.Maui.ViewModels;

public sealed partial class ConfirmationsOverviewViewModel : MyObservableRecipient
{
    public ConfirmationsOverviewViewModel(IConfirmationService confirmationServiceBase)
    {
        _confirmationServiceBase = confirmationServiceBase;
    }

    private readonly IConfirmationService _confirmationServiceBase;

    [ObservableProperty]
    private IReadOnlyList<SteamGuardAccountConfirmationsModel> _confirmations = Array.Empty<SteamGuardAccountConfirmationsModel>();

    [ObservableProperty]
    private bool _isRefreshing;

    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        Confirmations = Array.Empty<SteamGuardAccountConfirmationsModel>();
    }

    [RelayCommand]
    private async Task Refresh()
    {
        try
        {
            HapticFeedback.Perform(HapticFeedbackType.LongPress);
        }
        catch
        {
            //
        }

        Confirmations = await _confirmationServiceBase.CheckConfirmationFromAllAccounts().ConfigureAwait(false);

        IsRefreshing = false;
    }

    [RelayCommand]
    private async Task OnTouched(SteamGuardAccountConfirmationsModel model)
    {
        await Shell.Current.GoToAsync($"{nameof(AccountConfirmationsPage)}");

        Messenger.Send(new UpdateAccountConfirmationPageMessage(model));

        await Refresh();
    }
}