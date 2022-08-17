using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SteamAuthenticatorCore.Mobile.Pages;
using SteamAuthenticatorCore.Shared.Models;
using SteamAuthenticatorCore.Shared.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SteamAuthenticatorCore.Mobile.ViewModels;

public sealed partial class ConfirmationsOverviewViewModel : ObservableObject
{
    public ConfirmationsOverviewViewModel(ConfirmationServiceBase confirmationServiceBase)
    {
        ConfirmationServiceBase = confirmationServiceBase;
    }

    private bool _needRefresh;

    [ObservableProperty]
    private bool _isRefreshing;

    public ConfirmationServiceBase ConfirmationServiceBase { get; }

    [RelayCommand]
    private void OnAppearing()
    {
        if (_needRefresh)
        {
            _needRefresh = false;
            IsRefreshing = true;
        }

        if (ConfirmationServiceBase.Accounts.Count > 0)
            return;

        _needRefresh = true;
        IsRefreshing = true;
        _needRefresh = false;
    }

    [RelayCommand]
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

        await ConfirmationServiceBase.CheckConfirmations();
        IsRefreshing = false;
    }

    [RelayCommand]
    private Task OnTouched(ConfirmationAccountModelBase account)
    {
        _needRefresh = true;
        return Shell.Current.GoToAsync($"{nameof(ConfirmationsPage)}");
    }
}