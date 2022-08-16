namespace SteamAuthenticatorCore.Shared.Abstraction;

public interface ISettingsService
{
    public void LoadSettings(ISettings settings);
    public void SaveSettings(ISettings settings);

    public void SaveSetting(string fieldName, ISettings settings);
}