using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.Messages;
using Wpf.Ui.Mvvm.Contracts;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public partial class LoginViewModel : ObservableObject, IRecipient<UpdateAccountInLoginPageMessage>
{
    public LoginViewModel(INavigationService navigation, IMessenger messenger, ILoginService loginService, ISteamGuardAccountService accountService)
    {
        _navigation = navigation;
        _loginService = loginService;
        _accountService = accountService;
        messenger.Register(this);
    }

    private readonly INavigationService _navigation;
    private readonly ILoginService _loginService;
    private readonly ISteamGuardAccountService _accountService;

    [ObservableProperty]
    private SteamGuardAccount _account = null!;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isPasswordBoxEnabled = true;

    public void Receive(UpdateAccountInLoginPageMessage message)
    {
        Account = message.Value;
    }

    [RelayCommand]
    private async Task OnLogin()
    {
        IsPasswordBoxEnabled = false;

        await _loginService.RefreshLogin(Account, Password);
        _navigation.NavigateTo("..");

        IsPasswordBoxEnabled = true;
    }
}