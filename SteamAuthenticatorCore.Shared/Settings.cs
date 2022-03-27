using System;

namespace SteamAuthenticatorCore.Shared;

public sealed class IgnoreSettings : Attribute
{

}

public interface ISettingsService
{
    public void LoadSettings(ISettings settings);
    public void SaveSettings(ISettings settings);

    public void SaveSetting(string fieldName, ISettings settings);
}

public interface ISettings
{
    public ISettingsService SettingsService { get; }

    public void DefaultSettings();
    public void LoadSettings();
    public void SaveSettings();
}