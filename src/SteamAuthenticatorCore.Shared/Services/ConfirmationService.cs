using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Shared.Abstractions;

namespace SteamAuthenticatorCore.Shared.Services;

internal class ConfirmationService : IConfirmationService, IDisposable
{
    public ConfirmationService(ObservableCollection<SteamGuardAccount> steamGuardAccounts, AppSettings settings, ITaskTimer taskTimer, ISteamGuardAccountService accountService, IPlatformImplementations platformImplementations)
    {
        ConfirmationViewModels = new ObservableCollection<Models.ConfirmationModel>();

        _steamGuardAccounts = steamGuardAccounts;
        _taskTimer = taskTimer;
        _accountService = accountService;
        _platformImplementations = platformImplementations;
        _settings = settings;
    }

    private readonly ObservableCollection<SteamGuardAccount> _steamGuardAccounts;
    private readonly AppSettings _settings;
    private readonly ITaskTimer _taskTimer;
    private readonly ISteamGuardAccountService _accountService;
    private readonly IPlatformImplementations _platformImplementations;

    public ObservableCollection<Models.ConfirmationModel> ConfirmationViewModels { get; }

    public void Dispose()
    {
        _settings.PropertyChanged -= SettingsOnPropertyChanged;
    }

    public async void Initialize()
    {
        _settings.PropertyChanged += SettingsOnPropertyChanged;

        if (!_settings.AutoConfirmMarketTransactions)
            return;

        await _taskTimer.StartOrRestart(TimeSpan.FromSeconds(_settings.PeriodicCheckingInterval), TradeAutoConfirmationTimerOnTick).ConfigureAwait(false);
    }
    
    public async Task CheckConfirmations()
    {
        ConfirmationViewModels.Clear();

        await Parallel.ForEachAsync(_steamGuardAccounts, async (account, token) =>
        {
            var confirmations = (await _accountService.FetchConfirmations(account, token).ConfigureAwait(false)).ToArray();

            if (!confirmations.Any())
                return;

            var list = new List<ConfirmationModel>(confirmations.Length);
            foreach (var confirmation in confirmations)
            {
                confirmation.BitMapImage = _platformImplementations.CreateImage(confirmation.ImageSource);
                list.Add(confirmation);
            }

            await _platformImplementations.InvokeMainThread(() =>
            {
                ConfirmationViewModels.Add(new Models.ConfirmationModel(account, list));
            });
        });
    }

    private async void SettingsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var settings = (AppSettings) sender!;
        if (!settings.IsLoaded)
            return;

        switch (e.PropertyName)
        {
            case nameof(settings.PeriodicCheckingInterval):
                if (!settings.AutoConfirmMarketTransactions)
                    break;

                await _taskTimer.StartOrRestart(TimeSpan.FromSeconds(settings.PeriodicCheckingInterval), TradeAutoConfirmationTimerOnTick).ConfigureAwait(false);
                break;
            case nameof(settings.AutoConfirmMarketTransactions):
                switch (settings.AutoConfirmMarketTransactions)
                {
                    case true:
                        await _taskTimer.StartOrRestart(TimeSpan.FromSeconds(settings.PeriodicCheckingInterval), TradeAutoConfirmationTimerOnTick).ConfigureAwait(false);
                        break;
                    case false:
                        await _taskTimer.DisposeAsync().ConfigureAwait(false);
                        break;
                }
                break;
        }
    }

    private Task TradeAutoConfirmationTimerOnTick(CancellationToken cancellationToken) =>
        Parallel.ForEachAsync(_steamGuardAccounts, cancellationToken, async (account, token) =>
        {
            var confirmations = (await _accountService.FetchConfirmations(account, token).ConfigureAwait(false)).Where(model => model.ConfType == ConfirmationType.MarketSellTransaction).ToArray();

            if (confirmations.Length == 0)
                return;

            if (confirmations.Length == 1)
            {
                await _accountService.SendConfirmation(account, confirmations[0], ConfirmationOptions.Allow, token).ConfigureAwait(false);
                return;
            }

            await _accountService.SendConfirmation(account, confirmations, ConfirmationOptions.Allow, token).ConfigureAwait(false);
        });
}
