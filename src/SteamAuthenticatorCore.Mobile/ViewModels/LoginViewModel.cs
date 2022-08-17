using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAuthCore;
using SteamAuthenticatorCore.Shared.Messages;
using SteamAuthenticatorCore.Shared.Services;
using ObservableObject = CommunityToolkit.Mvvm.ComponentModel.ObservableObject;


namespace SteamAuthenticatorCore.Mobile.ViewModels;

public partial class LoginViewModel : ObservableObject, IRecipient<UpdateAccountInLoginPageMessage>
{
    public LoginViewModel(LoginService loginService, IMessenger messenger)
    {
        _loginService = loginService;
        messenger.Register(this);
    }

    private readonly LoginService _loginService;

    [ObservableProperty]
    private SteamGuardAccount _account = null!;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isPasswordBoxEnabled = true;

    [RelayCommand]
    public async Task OnLogin()
    {
        
    }

    public void Receive(UpdateAccountInLoginPageMessage message)
    {
        Account = message.Value;
    }
}