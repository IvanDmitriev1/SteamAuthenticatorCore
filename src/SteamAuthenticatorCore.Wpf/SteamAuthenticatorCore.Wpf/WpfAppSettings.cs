using Microsoft.Win32;
using SteamAuthenticatorCore.Shared;
using SteamAuthenticatorCore.Shared.Abstractions;
using System;
using System.Reflection;

namespace SteamAuthenticatorCore.Desktop;

public sealed class WpfAppSettings : AppSettings, IDisposable
{
    private WpfAppSettings()
    {
        var appName = Assembly.GetEntryAssembly()!.GetName().Name!;

        _registrySoftwareKey = Registry.CurrentUser.OpenSubKey("Software", true)!;
        _appRegistryKey = _registrySoftwareKey.OpenSubKey(appName, true) ??
                          _registrySoftwareKey.CreateSubKey(appName);
    }

    static WpfAppSettings()
    {
        var appSettings = new WpfAppSettings();
        AppSettings.Current = appSettings;
        Current = appSettings;
    }

    private readonly RegistryKey _registrySoftwareKey;
    private readonly RegistryKey _appRegistryKey;

    [IgnoreSetting]
    public new static WpfAppSettings Current { get; }

    public override void Load()
    {
        foreach (var propertyInfo in PropertyInfos)
        {
            var propertyValue = _appRegistryKey.GetValue(propertyInfo.Name);

            try
            {
                if (propertyValue is null)
                {
                    _appRegistryKey.SetValue(propertyInfo.Name, propertyInfo.GetValue(this, null) ?? propertyInfo.PropertyType);
                    continue;
                }

                try
                {
                    propertyValue = propertyInfo.PropertyType.IsEnum ? Enum.Parse(propertyInfo.PropertyType, (string)propertyValue) : Convert.ChangeType(propertyValue, propertyInfo.PropertyType);
                }
                catch (Exception)
                {
                    _appRegistryKey.SetValue(propertyInfo.Name, propertyInfo.GetValue(this, null) ?? propertyInfo.PropertyType);
                    return;
                }

                propertyInfo.SetValue(this, propertyValue);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to load setting", e);
            }
        }

        IsLoaded = true;
    }

    public override void Save()
    {
        foreach (var propertyInfo in PropertyInfos)
        {
            _appRegistryKey.SetValue(propertyInfo.Name, propertyInfo.GetValue(this) ?? propertyInfo.PropertyType);
        }
    }

    protected override void Save(string propertyName)
    {
        var propertyInfo = PropertiesDictionary[propertyName];
        _appRegistryKey.SetValue(propertyInfo.Name, propertyInfo.GetValue(this) ?? propertyInfo.PropertyType);
    }

    public void Dispose()
    {
        _registrySoftwareKey.Dispose();
        _appRegistryKey.Dispose();
    }
}