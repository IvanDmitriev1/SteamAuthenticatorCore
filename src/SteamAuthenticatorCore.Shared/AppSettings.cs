using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.Models;

namespace SteamAuthenticatorCore.Shared;

public abstract partial class AppSettings : AutoSettings
{
    [IgnoreSetting]
    public static AppSettings Current { get; protected set; } = null!;

    protected AppSettings()
    {
        AccountsLocation = AccountsLocationModel.LocalDrive;
        FirstRun = true;
        PeriodicCheckingInterval = 15;
        AutoConfirmMarketTransactions = false;
    }

    [ObservableProperty]
    private AccountsLocationModel _accountsLocation;
    
    [ObservableProperty]
    private bool _firstRun;

    [ObservableProperty]
    private int _periodicCheckingInterval;
    
    [ObservableProperty]
    private bool _autoConfirmMarketTransactions;

    [IgnoreSetting]
    public bool IsLoaded { get; protected set; }

    protected abstract void Save(string propertyName);

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (!IsLoaded)
            return;

        base.OnPropertyChanged(e);

        Save(e.PropertyName!);
    }
}