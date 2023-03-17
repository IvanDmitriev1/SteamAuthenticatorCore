using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAuthenticatorCore.Desktop.Services;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.ViewModel;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public partial class LoginViewModel : LoginViewModelBase
{
    public LoginViewModel(ILoginService loginService, IMessenger messenger) : base(loginService, messenger)
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