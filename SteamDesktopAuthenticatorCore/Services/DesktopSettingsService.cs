using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Win32;
using SteamAuthenticatorCore.Shared;

namespace SteamAuthenticatorCore.Desktop.Services;

internal sealed class DesktopSettingsService : ISettingsService
{
    public DesktopSettingsService()
    {
        _appName = Assembly.GetEntryAssembly()!.GetName().Name!;
    }

    private readonly string _appName;

    public void LoadSettings(ISettings settings)
    {
        var type = settings.GetType();
        var properties = type.GetProperties().SkipWhile(info => info.GetCustomAttribute<IgnoreSettings>() is not null);

        using var softwareKey = Registry.CurrentUser.OpenSubKey("Software", true)!;
        using var key = softwareKey.OpenSubKey(_appName, true) ?? softwareKey.CreateSubKey(_appName);

        using var typeKey = key.OpenSubKey(type.Name, true) ?? key.CreateSubKey(type.Name);

        foreach (var property in properties)
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
        var type = settings.GetType();
        var properties = type.GetProperties().SkipWhile(info => info.GetCustomAttribute<IgnoreSettings>() is not null);

        using var softwareKey = Registry.CurrentUser.OpenSubKey("Software", true)!;
        using var key = softwareKey.OpenSubKey(_appName, true) ?? softwareKey.CreateSubKey(_appName);

        using var typeKey = key.OpenSubKey(type.Name, true) ?? key.CreateSubKey(type.Name);

        foreach (var property in properties)
        {
            typeKey.SetValue(property.Name, property.GetValue(settings) ?? property.PropertyType);
        }
    }
}