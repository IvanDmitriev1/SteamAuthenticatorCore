using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public partial class LoginViewModel : MyObservableRecipient, IRecipient<UpdateAccountInLoginPageMessage>
{
    public LoginViewModel(ISteamGuardAccountService steamGuardAccountService, AccountsServiceResolver accountsServiceResolver)
    {
        _steamGuardAccountService = steamGuardAccountService;
        _accountsServiceResolver = accountsServiceResolver;
    }

    private readonly AccountsServiceResolver _accountsServiceResolver;
    private readonly ISteamGuardAccountService _steamGuardAccountService;
    private SteamGuardAccount? _steamGuardAccount;
    private LoginAgainData _loginAgainData = new();

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isPasswordBoxEnabled = true;


    [ObservableProperty]
    private bool _isCaptchaBoxVisible;

    [ObservableProperty]
    private string? _captchaText;

    [ObservableProperty]
    private ImageSource? _captchaImageSource;

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
        _loginAgainData.CaptchaText = CaptchaText;
        _loginAgainData = await _steamGuardAccountService.LoginAgain(_steamGuardAccount, Password, _loginAgainData, CancellationToken.None);

        switch (_loginAgainData.LoginResult)
        {
        case LoginResult.TooManyFailedLogins:
            await ContentDialogService.Default.ShowAlertAsync("Login", "To many requests try again later", "Ok");
            NavigationService.Default.GoBack();
            return;
        case LoginResult.LoginOkay:
            await _accountsServiceResolver.Invoke().Update(_steamGuardAccount);
            NavigationService.Default.GoBack();
            return;
        case LoginResult.NeedCaptcha:
            CaptchaImageSource = new BitmapImage(new Uri($"https://steamcommunity.com/public/captcha.php?gid={_loginAgainData.CaptchaGid}"));
            IsCaptchaBoxVisible = true;
            break;
        default: IsPasswordBoxEnabled = true;
            break;
        }
    }
}