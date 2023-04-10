namespace SteamAuthenticatorCore.Shared.Abstractions;

public interface ILocalizationProvider : INotifyPropertyChanged
{
    void ChangeLanguage(AvailableLanguage language);

    string this[string key] { get; }
    string this[LocalizationMessage message] { get; }
}