using System.Collections.ObjectModel;
using SteamAuthCore;
using SteamAuthCore.Manifest;
using SteamAuthenticatorCore.Shared.Services;
using Xamarin.CommunityToolkit.ObjectModel;


namespace SteamAuthenticatorCore.Mobile.ViewModels;

public class LoginViewModel : ObservableObject
{
    public LoginViewModel(ObservableCollection<SteamGuardAccount> accounts, IManifestModelService manifestModelService, LoginService loginService)
    {
        _accounts = accounts;
        _manifestModelService = manifestModelService;
        LoginService = loginService;
    }

    private readonly ObservableCollection<SteamGuardAccount> _accounts;
    private readonly IManifestModelService _manifestModelService;

    public LoginService LoginService { get; }
}