using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Input;
using SteamAuthCore;
using SteamAuthCore.Manifest;
using SteamAuthenticatorCore.Shared;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;


namespace SteamAuthenticatorCore.Mobile.ViewModels;

public class LoginViewModel : ObservableObject, IQueryAttributable
{
    public LoginViewModel()
    {
        _accounts = DependencyService.Get<ObservableCollection<SteamGuardAccount>>();
        _manifestModelService = DependencyService.Get<IManifestModelService>();

        LoginService = DependencyService.Get<LoginService>();
    }

    private readonly ObservableCollection<SteamGuardAccount> _accounts;
    private readonly IManifestModelService _manifestModelService;

    public LoginService LoginService { get; }

    public void ApplyQueryAttributes(IDictionary<string, string> query)
    {
        var id= HttpUtility.UrlDecode(query["id"]);

        LoginService.Account = _accounts[Convert.ToInt32(id)];
    }

    public ICommand LoginCommand => new SteamAuthenticatorCore.Shared.Helpers.AsyncRelayCommand(async () =>
    {
        if (LoginService.Account is null)
        {
            await Shell.Current.GoToAsync("..");
            return;
        }

        await RefreshLogin();
    });

    private async Task RefreshLogin()
    {
        switch (await LoginService.RefreshLogin())
        {
            case LoginResult.LoginOkay:
                await _manifestModelService.SaveSteamGuardAccount(LoginService.Account!);
                await Application.Current.MainPage.DisplayAlert("Login", "Login success", "Ok");
                break;
            case LoginResult.GeneralFailure:
                await Application.Current.MainPage.DisplayAlert("Login", "Error logging in: Steam returned \"GeneralFailure\"", "Ok");
                break;
            case LoginResult.BadRsa:
                await Application.Current.MainPage.DisplayAlert("Login", "Error logging in: Steam returned \"BadRSA\"", "Ok");
                break;
            case LoginResult.BadCredentials:
                await Application.Current.MainPage.DisplayAlert("Login", "Error logging in: Username or password was incorrect", "Ok");
                break;
            case LoginResult.TooManyFailedLogins:
                await Application.Current.MainPage.DisplayAlert("Login", "Error logging in: Too many failed logins, try again later", "Ok");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        await Shell.Current.GoToAsync("..");
    }
}