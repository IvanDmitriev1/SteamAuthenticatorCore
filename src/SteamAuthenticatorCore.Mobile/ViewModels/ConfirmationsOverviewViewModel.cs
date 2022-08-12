using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SteamAuthenticatorCore.Mobile.Pages;
using SteamAuthenticatorCore.Shared;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SteamAuthenticatorCore.Mobile.ViewModels;

public sealed partial class ConfirmationsOverviewViewModel : ObservableObject
{
    public ConfirmationsOverviewViewModel(BaseConfirmationService baseConfirmationService)
    {
        ConfirmationService = baseConfirmationService;
    }

    private bool _needRefresh;

    [ObservableProperty]
    private bool _isRefreshing;

    public BaseConfirmationService ConfirmationService { get; }

    [ICommand]
    private void OnAppearing()
    {
        if (_needRefresh)
        {
            _needRefresh = false;
            IsRefreshing = true;
        }

        if (ConfirmationService.Accounts.Count > 0)
            return;

        _needRefresh = true;
        IsRefreshing = true;
        _needRefresh = false;
    }

    [ICommand]
    private async Task Refresh()
    {
        if (!_needRefresh)
        {
            try
            {
                HapticFeedback.Perform(HapticFeedbackType.LongPress);
            }
            catch
            {
                //
            }
        }

        await ConfirmationService.CheckConfirmations();
        IsRefreshing = false;
    }

    [ICommand]
    private Task OnTouched(ConfirmationAccountBase account)
    {
        _needRefresh = true;
        return Shell.Current.GoToAsync($"{nameof(ConfirmationsPage)}?id={ConfirmationService.Accounts.IndexOf(account)}");
    }
}