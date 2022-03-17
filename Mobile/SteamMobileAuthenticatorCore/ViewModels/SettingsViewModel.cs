using CommunityToolkit.Mvvm.Input;
using SteamAuthenticatorCore.Shared;
using Xamarin.Forms;

namespace SteamAuthenticatorCore.Mobile.ViewModels;

public partial class SettingsViewModel
{
    public SettingsViewModel()
    {
        AppSettings = DependencyService.Get<AppSettings>();
    }

        
    public AppSettings AppSettings { get; }


    [ICommand]
    private void OnLoading()
    {
        //App.AutoMarketSellTimer.Stop();
    }

    [ICommand]
    private void OnClosing()
    {
        /*var manifest = _manifestModelService.GetManifestModel();
        manifest.AutoConfirmMarketTransactions = AutoConfirmMarket;
        manifest.PeriodicCheckingInterval = TradePeriodicCheckingInterval;*/

        //App.AutoMarketSellTimer.Start(TimeSpan.FromSeconds(manifest.PeriodicCheckingInterval));
    }
}