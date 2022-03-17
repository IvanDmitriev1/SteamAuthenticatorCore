using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using SteamAuthCore;

namespace SteamAuthenticatorCore.Shared;

public abstract class ConfirmationAccountBase
{
    protected ConfirmationAccountBase(SteamGuardAccount account, ConfirmationModel[] confirmations, IPlatformImplementations platformImplementations)
    {
        Account = account;
        _platformImplementations = platformImplementations;

        foreach (var confirmation in confirmations)
            confirmation.BitMapImage = platformImplementations.CreateImage(confirmation.ImageSource);

        Confirmations = new ObservableCollection<ConfirmationModel>(confirmations);
    }

    private readonly IPlatformImplementations _platformImplementations;

    public SteamGuardAccount Account { get; }
    public ObservableCollection<ConfirmationModel> Confirmations { get; }

    public abstract ICommand ConfirmCommand { get; }

    public abstract ICommand CancelCommand { get; }

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
        Accounts.Clear();

        foreach (var account in _steamGuardAccounts)
        {
            if (await CreateConfirmationAccount(account) is not { } confirmationAccount) continue;

            Accounts.Add(confirmationAccount);
        }
    }

    public async Task<ConfirmationAccountBase?> CreateConfirmationAccount(SteamGuardAccount account)
    {
        try
        {
            return await CreateConfirmationAccountViewModel(account);
        }
        catch (SteamGuardAccount.WgTokenInvalidException)
        {
            await account.RefreshSessionAsync();

            try
            {
                return await CreateConfirmationAccountViewModel(account);
            }
            catch (SteamGuardAccount.WgTokenInvalidException)
            {
            }
        }
        catch (SteamGuardAccount.WgTokenExpiredException)
        {
            
        }

        return default;
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


    protected abstract Task<ConfirmationAccountBase?> CreateConfirmationAccountViewModel(SteamGuardAccount account);
}