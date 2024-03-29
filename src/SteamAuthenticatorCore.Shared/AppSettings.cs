﻿namespace SteamAuthenticatorCore.Shared;

public abstract partial class AppSettings : AutoSettings
{
    protected AppSettings()
    {
        AccountsLocation = AccountsLocation.LocalDrive;
        FirstRun = true;
        PeriodicCheckingInterval = 20;
        AutoConfirmMarketTransactions = false;

        LocalizationProvider = new XmlLocalizationProvider(AvailableLanguage.English);

        Language = Thread.CurrentThread.CurrentUICulture.Name switch
        {
            "ru-RU" => AvailableLanguage.Russian,
            _ => AvailableLanguage.English
        };
    }

    [IgnoreSetting]
    public static AppSettings Current { get; protected set; } = null!;

    [ObservableProperty]
    private AccountsLocation _accountsLocation;
    
    [ObservableProperty]
    private bool _firstRun;

    [ObservableProperty]
    private int _periodicCheckingInterval;
    
    [ObservableProperty]
    private bool _autoConfirmMarketTransactions;

    [ObservableProperty]
    private AvailableLanguage _language;

    [IgnoreSetting]
    public bool IsLoaded { get; protected set; }

    [IgnoreSetting]
    public ILocalizationProvider LocalizationProvider { get; }

    protected abstract void Save(string propertyName);

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (!IsLoaded)
            return;

        base.OnPropertyChanged(e);

        Save(e.PropertyName!);
    }

    partial void OnLanguageChanged(AvailableLanguage value)
    {
        LocalizationProvider.ChangeLanguage(value);
    }
}