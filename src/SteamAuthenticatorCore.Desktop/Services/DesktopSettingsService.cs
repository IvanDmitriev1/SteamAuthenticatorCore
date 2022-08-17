using Microsoft.Win32;
using SteamAuthenticatorCore.Shared.Abstraction;
using SteamAuthenticatorCore.Shared;
using System.Diagnostics;
using System.Reflection;
using System;
using System.Linq;

namespace SteamAuthenticatorCore.Desktop.Services;

internal sealed class DesktopSettingsService : ISettingsService
{
    public DesktopSettingsService()
    {
        _appName = Assembly.GetEntryAssembly()!.GetName().Name!;
    }

    private readonly string _appName;
    private static readonly Type SettingsType = typeof(AppSettings);
    private static readonly PropertyInfo[] PropertyInfos = typeof(AppSettings).GetProperties().SkipWhile(info => info.GetCustomAttribute<IgnoreSetting>() is not null).ToArray();

    public void LoadSettings(ISettings settings)
    {
        using var softwareKey = Registry.CurrentUser.OpenSubKey("Software", true)!;
        using var key = softwareKey.OpenSubKey(_appName, true) ?? softwareKey.CreateSubKey(_appName);

        using var typeKey = key.OpenSubKey(SettingsType.Name, true) ?? key.CreateSubKey(SettingsType.Name);

        foreach (var property in PropertyInfos)
        {
            var typeValue = typeKey.GetValue(property.Name);

            try
            {
                if (typeValue is null)
                {
                    typeKey.SetValue(property.Name, property.GetValue(settings, null) ?? property.PropertyType);
                    return;
                }

                try
                {
                    typeValue = property.PropertyType.IsEnum ? Enum.Parse(property.PropertyType, (string) typeValue) : Convert.ChangeType(typeValue, property.PropertyType);
                }
                catch (Exception)
                {
                    typeKey.SetValue(property.Name, property.GetValue(settings, null) ?? property.PropertyType);
                    return;
                }

                property.SetValue(settings, typeValue);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw new Exception("Failed to load setting");
            }
        }
    }

    public void SaveSettings(ISettings settings)
    {
        using var softwareKey = Registry.CurrentUser.OpenSubKey("Software", true)!;
        using var key = softwareKey.OpenSubKey(_appName, true) ?? softwareKey.CreateSubKey(_appName);

        using var typeKey = key.OpenSubKey(SettingsType.Name, true) ?? key.CreateSubKey(SettingsType.Name);

        foreach (var property in PropertyInfos)
        {
            typeKey.SetValue(property.Name, property.GetValue(settings) ?? property.PropertyType);
        }
    }

    public void SaveSetting(string fieldName, ISettings settings)
    {
        using var softwareKey = Registry.CurrentUser.OpenSubKey("Software", true)!;
        using var key = softwareKey.OpenSubKey(_appName, true) ?? softwareKey.CreateSubKey(_appName);

        using var typeKey = key.OpenSubKey(SettingsType.Name, true) ?? key.CreateSubKey(SettingsType.Name);

        foreach (var property in PropertyInfos)
        {
            if (property.Name != fieldName) continue;

            typeKey.SetValue(property.Name, property.GetValue(settings) ?? property.PropertyType);
            return;
        }
    }
}