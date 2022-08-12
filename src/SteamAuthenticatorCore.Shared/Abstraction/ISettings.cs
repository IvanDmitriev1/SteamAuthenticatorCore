using System;

namespace SteamAuthenticatorCore.Shared.Abstraction;

public interface ISettings
{
    public ISettingsService SettingsService { get; }

    public void DefaultSettings();
    public void LoadSettings();
    public void SaveSettings();
}

public sealed class IgnoreSettings : Attribute
{

}