namespace SteamAuthenticatorCore.Shared.Abstraction;

public interface ISettingsService
{
    void LoadSettings(ISettings settings);
    void SaveSettings(ISettings settings);

    void SaveSetting(string fieldName, ISettings settings);
}