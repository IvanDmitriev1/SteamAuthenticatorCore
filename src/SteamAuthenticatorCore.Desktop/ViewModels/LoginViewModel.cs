using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAuthCore;
using SteamAuthenticatorCore.Desktop.Messages;
using SteamAuthenticatorCore.Shared.Services;
using Wpf.Ui.Mvvm.Contracts;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public partial class LoginViewModel : ObservableObject, IRecipient<UpdateAccountInLoginPageMessage>
{
    public LoginViewModel(INavigationService navigation, IMessenger messenger, LoginService loginService)
    {
        _navigation = navigation;
        _loginService = loginService;
        messenger.Register(this);
    }

    private readonly INavigationService _navigation;
    private readonly LoginService _loginService;

    [ObservableProperty]
    private SteamGuardAccount _account = null!;

    [ObservableProperty]
    private string _password = string.Empty;

    public void Receive(UpdateAccountInLoginPageMessage message)
    {
        Account = message.Value;
    }

    [RelayCommand]
    public Task OnLogin()
    {
        return _loginService.RefreshLogin(Account, Password);
    }
}