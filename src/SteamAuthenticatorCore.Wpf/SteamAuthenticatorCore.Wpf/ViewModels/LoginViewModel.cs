using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.ViewModel;
using Wpf.Ui.Contracts;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public partial class LoginViewModel : LoginViewModelBase
{
    public LoginViewModel(ILoginService loginService, IMessenger messenger, INavigationService navigation) : base(loginService, messenger)
    {
        _navigation = navigation;
    }

    private readonly INavigationService _navigation;

    [RelayCommand]
    protected async override Task OnLogin()
    {
        IsPasswordBoxEnabled = false;

        if (await LoginService.RefreshLogin(Account, Password))
            _navigation.GoBack();

        IsPasswordBoxEnabled = true;
    }
}