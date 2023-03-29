using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAuthenticatorCore.Mobile.Pages;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.Messages;
using SteamAuthenticatorCore.Shared.Models;

namespace SteamAuthenticatorCore.Mobile.ViewModels;

public sealed partial class ConfirmationsOverviewViewModel : ObservableRecipient
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