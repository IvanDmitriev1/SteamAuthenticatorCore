using System.Windows.Input;
using SteamAuthenticatorCore.Shared;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace SteamAuthenticatorCore.Mobile.ViewModels;

internal class ConfirmationViewModel : BaseViewModel
{
    public ConfirmationViewModel()
    {
        ConfirmationService = DependencyService.Get<BaseConfirmationService>();
    }

    public BaseConfirmationService ConfirmationService { get; }

    private bool _isRefreshing;

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
    }

    public ICommand RefreshCommand => new AsyncCommand(async () =>
    {
        IsRefreshing = true;

        await ConfirmationService.CheckConfirmations();

        IsRefreshing = false;
    });
}