using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.ViewModel;

namespace SteamAuthenticatorCore.Mobile.ViewModels;

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
