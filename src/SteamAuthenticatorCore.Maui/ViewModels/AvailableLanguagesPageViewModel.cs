namespace SteamAuthenticatorCore.Maui.ViewModels;

public sealed partial class AvailableLanguagesPageViewModel : MyObservableRecipient
{
    public AvailableLanguagesPageViewModel()
    {
        Settings = AppSettings.Current;
    }

    public AppSettings Settings { get; }
}