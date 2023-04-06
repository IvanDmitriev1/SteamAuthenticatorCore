namespace SteamAuthenticatorCore.Maui.ViewModels;

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
            await Shell.Current.GoToAsync("..");

        IsPasswordBoxEnabled = true;
    }
}
