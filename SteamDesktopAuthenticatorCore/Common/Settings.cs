using System;
using Microsoft.Extensions.DependencyInjection;
using SteamDesktopAuthenticatorCore.ViewModels;
using WpfHelper.Common;
using WpfHelper.Services;

namespace SteamDesktopAuthenticatorCore.Common
{
    public class AppSettings : BaseViewModel, ISettings
    {
        public AppSettings(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            DefaultSettings();
        }

        public enum ManifestLocationModel
        {
            None,
            LocalDrive,
            GoogleDrive
        }

        private readonly IServiceProvider _serviceProvider;
        private ManifestLocationModel _manifestLocation;
        private bool _firstRun;
        private bool _updated;
        private int _periodicCheckingInterval;
        private bool _autoConfirmMarketTransactions;


        public ManifestLocationModel ManifestLocation
        {
            get => _manifestLocation;
            set
            {
                if (!Set(ref _manifestLocation, value)) return;

                var manifestServiceResolver = _serviceProvider.GetRequiredService<App.ManifestServiceResolver>();

                var viewModel = _serviceProvider.GetRequiredService<TokenViewModel>();
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
                if (value < 10)
                    return;

                var confirmationViewModel = _serviceProvider.GetRequiredService<ConfirmationViewModel>();
                confirmationViewModel.ChangeTradeAutoConfirmationTimerInterval(value);

                Set(ref _periodicCheckingInterval, value);
            }
        }

        public bool AutoConfirmMarketTransactions
        {
            get => _autoConfirmMarketTransactions;
            set
            {
                var confirmationViewModel = _serviceProvider.GetRequiredService<ConfirmationViewModel>();
                confirmationViewModel.ChangeTradeAutoConfirmationTimerInterval(PeriodicCheckingInterval, value);

                Set(ref _autoConfirmMarketTransactions, value);
            }
        }

        public void DefaultSettings()
        {
            ManifestLocation = ManifestLocationModel.None;
            FirstRun = true;
            Updated = false;
            PeriodicCheckingInterval = 10;
            AutoConfirmMarketTransactions = false;
        }
    }
}
