using SteamAuthenticatorCore.Shared;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public class SettingsViewModel
{
    public SettingsViewModel(AppSettings settings)
    {
        Settings = settings;
    }

    public AppSettings Settings { get; }
}
