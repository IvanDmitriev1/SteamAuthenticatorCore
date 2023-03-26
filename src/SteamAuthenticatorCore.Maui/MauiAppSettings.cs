using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SteamAuthenticatorCore.Shared;
using SteamAuthenticatorCore.Shared.Abstractions;

namespace SteamAuthenticatorCore.Mobile;

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

        if (e.PropertyName != nameof(Theme))
            return;

        Application.Current!.UserAppTheme = Theme;
    }
}