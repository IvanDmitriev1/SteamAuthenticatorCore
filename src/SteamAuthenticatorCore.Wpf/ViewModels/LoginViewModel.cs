namespace SteamAuthenticatorCore.Desktop.ViewModels;

public partial class LoginViewModel : MyObservableRecipient, IRecipient<UpdateAccountInLoginPageMessage>
{
    public LoginViewModel(ILoginService loginService)
    {
        _loginService = loginService;
    }

    private readonly ILoginService _loginService;
    private SteamGuardAccount? _steamGuardAccount;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isPasswordBoxEnabled = true;

    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        _steamGuardAccount = null;
    }

    public void Receive(UpdateAccountInLoginPageMessage message)
    {
        _steamGuardAccount = message.Value;

        Username = _steamGuardAccount.AccountName;
    }

    [RelayCommand]
    private async Task OnLogin()
    {
        if (_steamGuardAccount is null)
            return;

        IsPasswordBoxEnabled = false;

        if (await _loginService.RefreshLogin(_steamGuardAccount, Password))
        {
            NavigationService.Default.GoBack();
            return;
        }

        IsPasswordBoxEnabled = true;
    }
}