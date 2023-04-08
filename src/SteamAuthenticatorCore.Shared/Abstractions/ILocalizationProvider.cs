namespace SteamAuthenticatorCore.Shared.Abstractions;

public interface ILocalizationProvider
{
    void ChangeLanguage(AvailableLanguages languages);

    string GetValue(LocalizationMessages  messages);
    string GetValue(string key);
}