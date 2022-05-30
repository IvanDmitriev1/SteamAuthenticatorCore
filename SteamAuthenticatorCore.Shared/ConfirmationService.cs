using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using SteamAuthCore;

namespace SteamAuthenticatorCore.Shared;

public abstract class ConfirmationAccountBase
{
    protected ConfirmationAccountBase(in SteamGuardAccount account,in IPlatformImplementations platformImplementations)
    {
        Account = account;
        _platformImplementations = platformImplementations;
        Confirmations = new ObservableCollection<ConfirmationModel>();
    }

    private readonly IPlatformImplementations _platformImplementations;

    public SteamGuardAccount Account { get; }
    public ObservableCollection<ConfirmationModel> Confirmations { get; }

    public abstract ICommand ConfirmCommand { get; }

    public abstract ICommand CancelCommand { get; }

    public async Task CheckConfirmations()
    {
        ConfirmationModel[] confirmations;

        try
        {
            confirmations = (await Account.FetchConfirmationsAsync()).ToArray();
        }
        catch (SteamGuardAccount.WgTokenInvalidException)
        {
            await Account.RefreshSessionAsync();

            try
            {
                confirmations = (await Account.FetchConfirmationsAsync()).ToArray();
            }
            catch (SteamGuardAccount.WgTokenInvalidException)
            {
                return;
            }
        }
        catch (SteamGuardAccount.WgTokenExpiredException)
        {
            return;
        }

        Confirmations.Clear();

        foreach (var confirmation in confirmations)
        {
            confirmation.BitMapImage = _platformImplementations.CreateImage(confirmation.ImageSource);
            Confirmations.Add(confirmation);
        }
    }

    public void SendConfirmation(ConfirmationModel confirmation, SteamGuardAccount.Confirmation command)
    {
        Account.SendConfirmationAjax(confirmation, command);

        _platformImplementations.InvokeMainThread(() =>
        {
            Confirmations.Remove(confirmation);
        });
    }

    public void SendConfirmations(ref IEnumerable<ConfirmationModel> confirmations, SteamGuardAccount.Confirmation command)
    {
        var confirmationModels = confirmations as ConfirmationModel[] ?? confirmations.ToArray();
        SendConfirmations(confirmationModels, command);
    }

    public void SendConfirmations(IReadOnlyCollection<ConfirmationModel> confirmations, SteamGuardAccount.Confirmation command)
    {
        Account.SendConfirmationAjax(confirmations, command);

        foreach (var confirmation in confirmations)
        {
            _platformImplementations.InvokeMainThread(() =>
            {
                Confirmations.Remove(confirmation);
            });
        }
    }
}

public abstract class BaseConfirmationService : IDisposable
{
    protected BaseConfirmationService(ObservableCollection<SteamGuardAccount> steamGuardAccounts, AppSettings settings, IPlatformImplementations platformImplementations, IPlatformTimer timer)
    {
        _steamGuardAccounts = steamGuardAccounts;
        _settings = settings;
        PlatformImplementations = platformImplementations;
        _timer = timer;
        Accounts = new ObservableCollection<ConfirmationAccountBase>();

        _timer.SetFuncOnTick(TradeAutoConfirmationTimerOnTick);
        _settings.PropertyChanged += SettingsOnPropertyChanged;
    }

    private readonly ObservableCollection<SteamGuardAccount> _steamGuardAccounts;
    private readonly AppSettings _settings;
    private readonly IPlatformTimer _timer;
    protected readonly IPlatformImplementations PlatformImplementations;

    public ObservableCollection<ConfirmationAccountBase> Accounts { get; }

    public void Dispose()
    {
        _settings.PropertyChanged -= SettingsOnPropertyChanged;
    }

    public async Task CheckConfirmations()
    {
        var tmpAccounts = new List<ConfirmationAccountBase>();
        foreach (var account in _steamGuardAccounts.Where(account => Accounts.All(confirmationAccount => !confirmationAccount.Account.Equals(account))))
        {
            var confirmationAccount = CreateConfirmationAccount(account);
            tmpAccounts.Add(confirmationAccount);
        }
        await Task.WhenAll(CreateFuncArray(Accounts));

        for (var i = Accounts.Count - 1; i >= 0; i--)
        {
            var account = Accounts[i];

            if (account.Confirmations.Count <= 0)
                Accounts.Remove(account);
        }

        await Task.WhenAll(CreateFuncArray(tmpAccounts));

        foreach (var account in tmpAccounts)
        {
            if (account.Confirmations.Count > 0)
                Accounts.Add(account);
        }
    }

    private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        var settings = (AppSettings) sender!;
        if (!settings.IsInitialized) return;

        switch (e.PropertyName)
        {
            case nameof(settings.PeriodicCheckingInterval):
                _timer.ChangeInterval(TimeSpan.FromSeconds(settings.PeriodicCheckingInterval));
                break;
            case nameof(settings.AutoConfirmMarketTransactions):
                switch (settings.AutoConfirmMarketTransactions)
                {
                    case true:
                        _timer.ChangeInterval(TimeSpan.FromSeconds(settings.PeriodicCheckingInterval));
                        _timer.Start();
                        break;
                    case false:
                        _timer.Stop();
                        break;
                }
                break;
        }
    }

    private async Task TradeAutoConfirmationTimerOnTick()
    {
        await CheckConfirmations();

        foreach (var confirmationAccountViewModel in Accounts)
        {
            var confirmations = new List<ConfirmationModel>();

            foreach (var confirmationModel in confirmationAccountViewModel.Confirmations)
                if (confirmationModel.ConfType == ConfirmationModel.ConfirmationType.MarketSellTransaction)
                    confirmations.Add(confirmationModel);

            confirmationAccountViewModel.SendConfirmations(confirmations, SteamGuardAccount.Confirmation.Allow);
        }
    }

    private static Task[] CreateFuncArray(in IReadOnlyCollection<ConfirmationAccountBase> confirmationAccountBases)
    {
        var funcs = new Task[confirmationAccountBases.Count];

        for (var i = 0; i < confirmationAccountBases.Count; i++)
        {
            var account = confirmationAccountBases.ElementAt(i);
            funcs[i] = account.CheckConfirmations();
        }

        return funcs;
    }

    public abstract ConfirmationAccountBase CreateConfirmationAccount(in SteamGuardAccount account);
}