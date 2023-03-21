using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Octokit;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.Models;

namespace SteamAuthenticatorCore.Shared.Services;

internal class ConfirmationService : IConfirmationService, IDisposable
{
    public ConfirmationService(ITaskTimer taskTimer, ISteamGuardAccountService accountService, IPlatformImplementations platformImplementations, AccountsServiceResolver accountsServiceResolver)
    {
        Confirmations = new ObservableCollection<SteamGuardAccountConfirmationsModel>();

        _taskTimer = taskTimer;
        _accountService = accountService;
        _platformImplementations = platformImplementations;
        _accountsServiceResolver = accountsServiceResolver;

        AppSettings.Current.PropertyChanged += SettingsOnPropertyChanged;
    }

    private readonly ITaskTimer _taskTimer;
    private readonly ISteamGuardAccountService _accountService;
    private readonly IPlatformImplementations _platformImplementations;
    private readonly AccountsServiceResolver _accountsServiceResolver;

    private IAccountsService _accountsService = null!;
    private IReadOnlyList<SteamGuardAccount> _accounts = Array.Empty<SteamGuardAccount>();

    public ObservableCollection<SteamGuardAccountConfirmationsModel> Confirmations { get; }

    public async Task Initialize()
    {
        await _accountsService.GetAll();

        if (!AppSettings.Current.AutoConfirmMarketTransactions)
            return;

        _accountsService = _accountsServiceResolver.Invoke();
        _accounts = await _accountsService.GetAll();

        await _taskTimer.StartOrRestart(TimeSpan.FromSeconds(AppSettings.Current.PeriodicCheckingInterval), TradeAutoConfirmationTimerOnTick);
    }

    public void Dispose()
    {
        AppSettings.Current.PropertyChanged -= SettingsOnPropertyChanged;
    }
    
    public async Task CheckConfirmations()
    {
        Confirmations.Clear();

        /*await Parallel.ForEachAsync(_steamGuardAccounts, async (account, token) =>
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
                Confirmations.Add(new SteamGuardAccountConfirmationsModel(account, list));
            });
        });*/
    }

    private async void SettingsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var settings = (AppSettings) sender!;
        if (!settings.IsLoaded)
            return;

        if (e.PropertyName == nameof(settings.AccountsLocation))
        {
            
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

    private async Task TradeAutoConfirmationTimerOnTick(CancellationToken cancellationToken)
    {
        /*return Parallel.ForEachAsync(_steamGuardAccounts, cancellationToken, async (account, token) =>
        {
            var confirmations = (await _accountService.FetchConfirmations(account, token).ConfigureAwait(false))
                .Where(model => model.ConfType == ConfirmationType.MarketSellTransaction).ToArray();

            if (confirmations.Length == 0)
                return;

            if (confirmations.Length == 1)
            {
                await _accountService.SendConfirmation(account, confirmations[0], ConfirmationOptions.Allow, token).ConfigureAwait(false);
                return;
            }

            await _accountService.SendConfirmation(account, confirmations, ConfirmationOptions.Allow, token).ConfigureAwait(false);
        });*/
    }
}
