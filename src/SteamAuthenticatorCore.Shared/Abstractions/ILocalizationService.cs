namespace SteamAuthenticatorCore.Shared.Abstractions;

public interface ILocalizationService
{
    void SetLanguage(AvailableLanguages  language);
}