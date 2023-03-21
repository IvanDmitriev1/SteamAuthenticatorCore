using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.Models;

namespace SteamAuthenticatorCore.Shared.Services;

internal class ConfirmationService : IConfirmationService, IDisposable
{
    public ConfirmationService(ITaskTimer taskTimer, ISteamGuardAccountService steamAccountService, IPlatformImplementations platformImplementations, AccountsServiceResolver accountsServiceResolver)
    {
        _taskTimer = taskTimer;
        _steamAccountService = steamAccountService;
        _platformImplementations = platformImplementations;
        _accountsServiceResolver = accountsServiceResolver;

        AppSettings.Current.PropertyChanged += SettingsOnPropertyChanged;
    }

    private readonly ITaskTimer _taskTimer;
    private readonly ISteamGuardAccountService _steamAccountService;
    private readonly IPlatformImplementations _platformImplementations;
    private readonly AccountsServiceResolver _accountsServiceResolver;

    private IAccountsService _accountsService = null!;

    public async Task Initialize()
    {
        _accountsService = _accountsServiceResolver.Invoke();

        if (!AppSettings.Current.AutoConfirmMarketTransactions)
            return;

        await _taskTimer.StartOrRestart(TimeSpan.FromSeconds(AppSettings.Current.PeriodicCheckingInterval), TradeAutoConfirmationTimerOnTick);
    }

    public void Dispose()
    {
        AppSettings.Current.PropertyChanged -= SettingsOnPropertyChanged;
    }
    
    public async Task<IReadOnlyList<SteamGuardAccountConfirmationsModel>> CheckConfirmationFromAllAccounts()
    {
        var accountConfirmations = new List<SteamGuardAccountConfirmationsModel>();

        foreach (var account in await _accountsService.GetAll().ConfigureAwait(false))
        {
            var confirmations = (await _steamAccountService.FetchConfirmations(account, CancellationToken.None).ConfigureAwait(false)).ToArray();
            if (!confirmations.Any())
                continue;

            foreach (var confirmation in confirmations)
            {
                confirmation.BitMapImage = _platformImplementations.CreateImage(confirmation.ImageSource);
            }

            accountConfirmations.Add(new SteamGuardAccountConfirmationsModel(account, confirmations));
        }

        return accountConfirmations;
    }

    private async Task TradeAutoConfirmationTimerOnTick(CancellationToken cancellationToken)
    {
        foreach (var account in await _accountsService.GetAll().ConfigureAwait(false))
        {
            var confirmations = await _steamAccountService.FetchConfirmations(account, cancellationToken).ConfigureAwait(false);
            var sortedConfirmations = confirmations.Where(model => model.ConfType == ConfirmationType.MarketSellTransaction).ToArray();

            if (sortedConfirmations.Length == 0)
                continue;

            if (sortedConfirmations.Length == 1)
            {
                await _steamAccountService.SendConfirmation(account, sortedConfirmations[0], ConfirmationOptions.Allow, cancellationToken).ConfigureAwait(false);
                continue;
            }

            await _steamAccountService.SendConfirmation(account, sortedConfirmations, ConfirmationOptions.Allow, cancellationToken).ConfigureAwait(false);
        }
    }

    private async void SettingsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var settings = (AppSettings) sender!;
        if (!settings.IsLoaded)
            return;

        if (e.PropertyName == nameof(settings.AccountsLocation))
        {
            _accountsService = _accountsServiceResolver.Invoke();
            return;
        }

        if (e.PropertyName == nameof(settings.PeriodicCheckingInterval))
        {
            if (!settings.AutoConfirmMarketTransactions)
                return;

            await _taskTimer.StartOrRestart(TimeSpan.FromSeconds(settings.PeriodicCheckingInterval), TradeAutoConfirmationTimerOnTick).ConfigureAwait(false);
            return;
        }

        if (e.PropertyName != nameof(settings.AutoConfirmMarketTransactions))
            return;

        if (settings.AutoConfirmMarketTransactions)
            await _taskTimer.StartOrRestart(TimeSpan.FromSeconds(settings.PeriodicCheckingInterval), TradeAutoConfirmationTimerOnTick).ConfigureAwait(false);
        else
            await _taskTimer.Stop().ConfigureAwait(false);
    }
}
