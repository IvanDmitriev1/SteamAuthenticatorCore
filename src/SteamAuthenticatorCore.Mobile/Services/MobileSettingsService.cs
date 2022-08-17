using System;
using System.Linq;
using System.Reflection;
using SteamAuthenticatorCore.Shared;
using SteamAuthenticatorCore.Shared.Abstraction;
using Xamarin.Essentials;

namespace SteamAuthenticatorCore.Mobile.Services;

internal class MobileSettingsService : ISettingsService
{
    private static readonly PropertyInfo[] PropertyInfos = typeof(AppSettings).GetProperties().SkipWhile(info => info.GetCustomAttribute<IgnoreSettings>() is not null).ToArray();

    public void LoadSettings(ISettings settings)
    {
        foreach (var property in PropertyInfos)
        {
            var value = property.GetValue(settings).ToString();
            value = Preferences.Get(property.Name, value);

            var typeValue = property.PropertyType.IsEnum ? Enum.Parse(property.PropertyType, value) : Convert.ChangeType(value, property.PropertyType);
            property.SetValue(settings, typeValue);
        }
    }

    public void SaveSettings(ISettings settings)
    {
        foreach (var property in PropertyInfos)
        {
            Preferences.Set(property.Name, property.GetValue(settings).ToString());
        }
    }

    public void SaveSetting(string fieldName, ISettings settings)
    {
        foreach (var property in PropertyInfos)
        {
            if (property.Name != fieldName) continue;

            Preferences.Set(property.Name, property.GetValue(settings).ToString());
            return;
        }
    }
}