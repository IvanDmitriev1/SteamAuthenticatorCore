using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.Messages;

namespace SteamAuthenticatorCore.Mobile.ViewModels;

public partial class LoginViewModel : ObservableObject, IRecipient<UpdateAccountInLoginPageMessage>
{
    public LoginViewModel(ILoginService loginService, IMessenger messenger)
    {
        _loginService = loginService;
        messenger.Register(this);
    }

    private readonly ILoginService _loginService;

    [ObservableProperty]
    private SteamGuardAccount _account = null!;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isPasswordBoxEnabled = true;

    [RelayCommand]
    public async Task OnLogin()
    {
        IsPasswordBoxEnabled = false;

        await _loginService.RefreshLogin(Account, Password);

        IsPasswordBoxEnabled = true;
    }

    public void Receive(UpdateAccountInLoginPageMessage message)
    {
        Account = message.Value;
    }
}
