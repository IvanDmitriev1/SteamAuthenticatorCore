using System;
using System.Threading.Tasks;

namespace SteamAuthenticatorCore.Shared;

public sealed class IgnoreSettings : Attribute
{

}

public interface ISettingsService
{
    public void LoadSettings(ISettings settings);
    public Task LoadSettingsAsync(ISettings settings);

    public void SaveSettings(ISettings settings);
    public Task SaveSettingsAsync(ISettings settings);
}

public interface ISettings
{
    public void DefaultSettings();
    public void LoadSettings();
    public void SaveSettings();
}