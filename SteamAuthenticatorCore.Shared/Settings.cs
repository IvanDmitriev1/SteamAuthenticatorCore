using System;

namespace SteamAuthenticatorCore.Shared;

public sealed class IgnoreSettings : Attribute
{

}

public interface ISettingsService
{
    public void LoadSettings(ISettings settings);
    public void SaveSettings(ISettings settings);
}

public interface ISettings
{
    public void DefaultSettings();
    public void LoadSettings();
    public void SaveSettings();
}