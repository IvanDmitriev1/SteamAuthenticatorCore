namespace SteamAuthenticatorCore.Shared.ViewModel;

public abstract partial class LoginViewModelBase : ObservableRecipient, IRecipient<UpdateAccountInLoginPageMessage>
{
    protected LoginViewModelBase(ILoginService loginService)
    {
        LoginService = loginService;
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
