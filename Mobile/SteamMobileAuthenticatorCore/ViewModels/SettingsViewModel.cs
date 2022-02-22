using System.Windows.Input;
using SteamAuthenticatorCore.Shared;
using Xamarin.Forms;

namespace SteamAuthenticatorCore.Mobile.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public SettingsViewModel()
        {
            AppSettings = DependencyService.Get<AppSettings>();
        }

        
        public AppSettings AppSettings { get; }

        public ICommand OnLoading => new Command(() =>
        {
            //App.AutoMarketSellTimer.Stop();
        });

        public ICommand OnClosingCommand => new Command(() =>
        {
            /*var manifest = _manifestModelService.GetManifestModel();
            manifest.AutoConfirmMarketTransactions = AutoConfirmMarket;
            manifest.PeriodicCheckingInterval = TradePeriodicCheckingInterval;*/

            //App.AutoMarketSellTimer.Start(TimeSpan.FromSeconds(manifest.PeriodicCheckingInterval));
        });
    }
}
