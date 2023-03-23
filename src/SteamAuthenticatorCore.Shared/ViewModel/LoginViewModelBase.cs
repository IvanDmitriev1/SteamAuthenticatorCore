using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.Messages;

namespace SteamAuthenticatorCore.Shared.ViewModel;

public abstract partial class LoginViewModelBase : ObservableRecipient, IRecipient<UpdateAccountInLoginPageMessage>
{
    protected LoginViewModelBase(ILoginService loginService, IMessenger messenger)
    {
        LoginService = loginService;
        messenger.Register(this);
    }

    protected readonly ILoginService LoginService;

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

    protected abstract Task OnLogin();
}
