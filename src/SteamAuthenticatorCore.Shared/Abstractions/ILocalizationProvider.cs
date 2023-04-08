namespace SteamAuthenticatorCore.Shared.Abstractions;

public interface ILocalizationProvider
{
    IReadOnlyDictionary<string, string> CurrentLanguageDictionary { get; }

    void ChangeLanguage(AvailableLanguages languages);
}