namespace SteamAuthenticatorCore.Shared.Abstractions;

public interface ILocalizationProvider : INotifyPropertyChanged
{
    void ChangeLanguage(AvailableLanguages languages);

    string this[string key] { get; }
}