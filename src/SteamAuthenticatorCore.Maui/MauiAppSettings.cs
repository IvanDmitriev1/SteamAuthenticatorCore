﻿using System.ComponentModel;

namespace SteamAuthenticatorCore.Maui;

public sealed partial class MauiAppSettings : AppSettings
{
    private MauiAppSettings() { }

    static MauiAppSettings()
    {
        var mauiAppSettings = new MauiAppSettings();
        Current = mauiAppSettings;
        AppSettings.Current = mauiAppSettings;
    }

    [IgnoreSetting]
    public new static MauiAppSettings Current { get; }

    [ObservableProperty]
    private AppTheme _theme;

    public override void Load()
    {
        foreach (var propertyInfo in PropertyInfos)
        {
            var value = propertyInfo.GetValue(this)!.ToString();
            value = Preferences.Get(propertyInfo.Name, value);

            var typeValue = propertyInfo.PropertyType.IsEnum ? Enum.Parse(propertyInfo.PropertyType, value!) : Convert.ChangeType(value, propertyInfo.PropertyType);
            propertyInfo.SetValue(this, typeValue);
        }

        IsLoaded = true;
        Application.Current!.UserAppTheme = Theme;
    }

    public override void Save()
    {
        foreach (var propertyInfo in PropertyInfos)
        {
            Preferences.Set(propertyInfo.Name, propertyInfo.GetValue(this)!.ToString());
        }
    }

    protected override void Save(string propertyName)
    {
        var propertyInfo = PropertiesDictionary[propertyName];
        Preferences.Set(propertyInfo.Name, propertyInfo.GetValue(this)!.ToString());
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (!IsLoaded)
            return;

        /*if (e.PropertyName == nameof(Language))
        {
            foreach (var shellItem in Shell.Current.Items[0].Items)
            {
                var message = ShellContentAttachedProperties.GetLocalizationMessage(shellItem);
                shellItem.Title = LocalizationProvider[message.ToString()];
            }
        }*/
    }

    partial void OnThemeChanged(AppTheme value)
    {
        Application.Current!.UserAppTheme = Theme;   
    }
}