using System;

namespace SteamAuthenticatorCore.Shared.Abstraction;

public interface ISettings
{
    ISettingsService SettingsService { get; }

    void DefaultSettings();
    void LoadSettings();
    void SaveSettings();
}

public sealed class IgnoreSetting : Attribute
{

}