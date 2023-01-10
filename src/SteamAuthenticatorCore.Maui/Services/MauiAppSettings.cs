using SteamAuthenticatorCore.Shared;
using SteamAuthenticatorCore.Shared.Abstractions;

namespace SteamAuthenticatorCore.Mobile.Services;

public sealed class MauiAppSettings : AppSettings
{
    public MauiAppSettings(IPlatformImplementations platformImplementations)
    {
        _platformImplementations = platformImplementations;
    }

    private readonly IPlatformImplementations _platformImplementations;

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

        if (nameof(Theme) != propertyName)
            return;

        _platformImplementations.SetTheme(Theme);
    }
}