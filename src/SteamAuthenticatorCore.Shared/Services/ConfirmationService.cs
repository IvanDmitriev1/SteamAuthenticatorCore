using System;
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
    public ConfirmationService(ObservableCollection<SteamGuardAccount> steamGuardAccounts, AppSettings settings, ITimer timer, ISteamGuardAccountService accountService, IConfirmationViewModelFactory confirmationViewModelFactory)
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
    private readonly ITimer _timer;
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

        _timer.StartOrRestart(TimeSpan.FromSeconds(_settings.PeriodicCheckingInterval), TradeAutoConfirmationTimerOnTick);
    }
    
    public async ValueTask CheckConfirmations()
    {
        ConfirmationViewModels.Clear();

        foreach (var account in _steamGuardAccounts)
        {
            var confirmations = (await _accountService.FetchConfirmations(account, CancellationToken.None)).ToArray();

            if (confirmations.Length == 0)
                continue;

            ConfirmationViewModels.Add(_confirmationViewModelFactory.Create(account, confirmations));
        }
    }

    private async void SettingsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var settings = (AppSettings) sender!;
        if (!settings.IsInitialized)
            return;

        switch (e.PropertyName)
        {
            case nameof(settings.PeriodicCheckingInterval):
                if (!settings.AutoConfirmMarketTransactions)
                    break;

                await _timer.StartOrRestart(TimeSpan.FromSeconds(settings.PeriodicCheckingInterval), TradeAutoConfirmationTimerOnTick).ConfigureAwait(false);
                break;
            case nameof(settings.AutoConfirmMarketTransactions):
                switch (settings.AutoConfirmMarketTransactions)
                {
                    case true:
                        await _timer.StartOrRestart(TimeSpan.FromSeconds(settings.PeriodicCheckingInterval), TradeAutoConfirmationTimerOnTick).ConfigureAwait(false);
                        break;
                    case false:
                        await _timer.DisposeAsync().ConfigureAwait(false);
                        break;
                }
                break;
        }
    }

    private async ValueTask TradeAutoConfirmationTimerOnTick(CancellationToken cancellationToken)
    {
        foreach (var account in _steamGuardAccounts)
        {
            var confirmations = (await _accountService.FetchConfirmations(account, cancellationToken).ConfigureAwait(false)).Where(model => model.ConfType == ConfirmationType.MarketSellTransaction).ToArray();

            if (confirmations.Length == 0)
                continue;

            if (confirmations.Length == 1)
            {
                await _accountService.SendConfirmation(account, confirmations[0], ConfirmationOptions.Allow, cancellationToken).ConfigureAwait(false);
                continue;
            }

            await _accountService.SendConfirmation(account, confirmations, ConfirmationOptions.Allow, cancellationToken).ConfigureAwait(false);
        }
    }
}
