using Microsoft.Extensions.DependencyInjection;
using SteamDesktopAuthenticatorCore.ViewModels;
using WpfHelper.Common;
using WpfHelper.Services;

namespace SteamDesktopAuthenticatorCore.Common
{
    public class AppSettings : BaseViewModel, ISettings
    {
        public AppSettings()
        {
            DefaultSettings();
        }

        public enum ManifestLocationModel
        {
            None,
            Drive,
            GoogleDrive
        }


        private ManifestLocationModel _manifestLocation;
        private bool _firstRun;
        private bool _updated;
        private int _periodicCheckingInterval;
        private bool _autoConfirmMarketTransactions;
        private bool _checkAllAccounts;


        public ManifestLocationModel ManifestLocation
        {
            get => _manifestLocation;
            set
            {
                if (!Set(ref _manifestLocation, value)) return;

                var manifestServiceResolver = App.ServiceProvider.GetRequiredService<App.ManifestServiceResolver>();

                var viewModel = App.ServiceProvider.GetRequiredService<TokenViewModel>();
                viewModel.UpdateManifestService(manifestServiceResolver.Invoke());
            }
        }

        public bool FirstRun
        {
            get => _firstRun;
            set => Set(ref _firstRun, value);
        }

        public bool Updated
        {
            get => _updated;
            set => Set(ref _updated, value);
        }

        public int PeriodicCheckingInterval
        {
            get => _periodicCheckingInterval;
            set
            {
                if (value == 0)
                    return;

                Set(ref _periodicCheckingInterval, value);
            }
        }

        public bool AutoConfirmMarketTransactions
        {
            get => _autoConfirmMarketTransactions;
            set => Set(ref _autoConfirmMarketTransactions, value);
        }

        public bool CheckAllAccounts
        {
            get => _checkAllAccounts;
            set => Set(ref _checkAllAccounts, value);
        }


        public void DefaultSettings()
        {
            ManifestLocation = ManifestLocationModel.None;
            FirstRun = true;
            Updated = false;
            PeriodicCheckingInterval = 10;
            AutoConfirmMarketTransactions = false;
            CheckAllAccounts = true;
        }
    }
}
