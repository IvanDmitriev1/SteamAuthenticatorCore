using System;
using SteamAuthCore;
using SteamAuthenticatorCore.Shared.Abstraction;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using SteamAuthenticatorCore.Shared.Models;
using System.Collections.Generic;

namespace SteamAuthenticatorCore.Shared.Services;

public abstract class ConfirmationServiceBase : IConfirmationService, IDisposable
{
    protected ConfirmationServiceBase(ObservableCollection<SteamGuardAccount> steamGuardAccounts, AppSettings settings, IPlatformImplementations platformImplementations, IPlatformTimer timer)
    {
        Accounts = new ObservableCollection<ConfirmationAccountModelBase>();

        _steamGuardAccounts = steamGuardAccounts;
        _platformImplementations = platformImplementations;
        _timer = timer;

        _settings = settings;
    }

    private readonly ObservableCollection<SteamGuardAccount> _steamGuardAccounts;
    private readonly AppSettings _settings;
    private readonly IPlatformImplementations _platformImplementations;
    private readonly IPlatformTimer _timer;

    public ObservableCollection<ConfirmationAccountModelBase> Accounts { get; }

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
        Accounts.Clear();

        foreach (var account in _steamGuardAccounts)
        {
            var confirmations = await ConfirmationAccountModelBase.TryGetConfirmations(account);

            if (confirmations.Length == 0)
                continue;

            Accounts.Add(CreateConfirmationAccountViewModel(account, confirmations, _platformImplementations));
        }
    }

    protected abstract ConfirmationAccountModelBase CreateConfirmationAccountViewModel(SteamGuardAccount account, ConfirmationModel[] confirmation, IPlatformImplementations platformImplementations);

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

        await CheckConfirmations();

        foreach (var confirmationAccountViewModel in Accounts)
        {
            var confirmations = new List<ConfirmationModel>();

            for (var i = 0; i < confirmationAccountViewModel.Confirmations.Count; i++)
            {
                var confirmationModel = confirmationAccountViewModel.Confirmations[i];

                if (confirmationModel.ConfType == ConfirmationModel.ConfirmationType.MarketSellTransaction)
                    confirmations.Add(confirmationModel);
            }

            await confirmationAccountViewModel.SendConfirmations(confirmations, SteamGuardAccount.Confirmation.Allow);
        }
    }
}
