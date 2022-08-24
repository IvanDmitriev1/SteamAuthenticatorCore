using System;
using SteamAuthCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Shared.Abstractions;

namespace SteamAuthenticatorCore.Shared.Services;

internal class ConfirmationService : IConfirmationService, IDisposable
{
    public ConfirmationService(ObservableCollection<SteamGuardAccount> steamGuardAccounts, AppSettings settings, IPlatformTimer timer, ISteamGuardAccountService accountService, IConfirmationViewModelFactory confirmationViewModelFactory)
    {
        ConfirmationViewModels = new ObservableCollection<IConfirmationViewModel>();

        _steamGuardAccounts = steamGuardAccounts;
        _timer = timer;
        _accountService = accountService;
        _confirmationViewModelFactory = confirmationViewModelFactory;
        _settings = settings;
    }

    private readonly ObservableCollection<SteamGuardAccount> _steamGuardAccounts;
    private readonly AppSettings _settings;
    private readonly IPlatformTimer _timer;
    private readonly ISteamGuardAccountService _accountService;
    private readonly IConfirmationViewModelFactory _confirmationViewModelFactory;

    public ObservableCollection<IConfirmationViewModel> ConfirmationViewModels { get; }

    public void Dispose()
    {
        _settings.PropertyChanged -= SettingsOnPropertyChanged;
    }

    public void Initialize()
    {
        _settings.PropertyChanged += SettingsOnPropertyChanged;

        if (!_settings.AutoConfirmMarketTransactions)
            return;

        _timer.Initialize(TimeSpan.FromSeconds(_settings.PeriodicCheckingInterval), TradeAutoConfirmationTimerOnTick);
        _timer.Start();
    }
    
    public async ValueTask CheckConfirmations()
    {
        ConfirmationViewModels.Clear();

        foreach (var account in _steamGuardAccounts)
        {
            var confirmations = (await _accountService.FetchConfirmations(account)).ToArray();

            if (confirmations.Length == 0)
                continue;

            ConfirmationViewModels.Add(_confirmationViewModelFactory.Create(account, confirmations));
        }
    }

    private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var settings = (AppSettings) sender!;
        if (!settings.IsInitialized)
            return;

        switch (e.PropertyName)
        {
            case nameof(settings.PeriodicCheckingInterval):
                if (!settings.AutoConfirmMarketTransactions)
                    break;

                _timer.Initialize(TimeSpan.FromSeconds(settings.PeriodicCheckingInterval), TradeAutoConfirmationTimerOnTick);
                _timer.Start();
                break;
            case nameof(settings.AutoConfirmMarketTransactions):
                switch (settings.AutoConfirmMarketTransactions)
                {
                    case true:
                        _timer.Initialize(TimeSpan.FromSeconds(settings.PeriodicCheckingInterval), TradeAutoConfirmationTimerOnTick);
                        _timer.Start();
                        break;
                    case false:
                        _timer.Stop();
                        break;
                }
                break;
        }
    }

    private async ValueTask TradeAutoConfirmationTimerOnTick(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        foreach (var account in _steamGuardAccounts)
        {
            var confirmations = (await _accountService.FetchConfirmations(account)).Where(model => model.ConfType == ConfirmationType.MarketSellTransaction).ToArray();

            if (confirmations.Length == 0)
                continue;

            if (confirmations.Length == 1)
            {
                await _accountService.SendConfirmation(account, confirmations[0], ConfirmationOptions.Allow);
                continue;
            }

            await _accountService.SendConfirmation(account, confirmations, ConfirmationOptions.Allow);
        }
    }
}
