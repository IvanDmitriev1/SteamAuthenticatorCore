namespace SteamAuthenticatorCore.Desktop.ViewModels;

public partial class LoginViewModel : LoginViewModelBase
{
    public LoginViewModel(ILoginService loginService) : base(loginService)
    {
    }

    [RelayCommand]
    protected override async Task OnLogin()
    {
        IsPasswordBoxEnabled = false;

        if (await LoginService.RefreshLogin(Account, Password))
            NavigationService.Default.GoBack();

        IsPasswordBoxEnabled = true;
    }
}