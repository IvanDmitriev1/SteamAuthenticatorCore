using Microsoft.Extensions.Logging;

namespace SteamAuthenticatorCore.Maui.ViewModels;

public partial class LoginViewModel : MyObservableRecipient, IRecipient<UpdateAccountInLoginPageMessage>
{
    public LoginViewModel(ISteamGuardAccountService steamGuardAccountService, AccountsServiceResolver accountsServiceResolver, ILogger<LoginViewModel> logger)
    {
        _steamGuardAccountService = steamGuardAccountService;
        _accountsServiceResolver = accountsServiceResolver;
        _logger = logger;
    }

    private readonly ISteamGuardAccountService _steamGuardAccountService;
    private readonly AccountsServiceResolver _accountsServiceResolver;
    private readonly ILogger<LoginViewModel> _logger;

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

        try
        {
            _loginAgainData = await _steamGuardAccountService.LoginAgain(_steamGuardAccount, Password, _loginAgainData, CancellationToken.None);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in OnLogin method");
            _loginAgainData = new LoginAgainData();
        }
        

        switch (_loginAgainData.LoginResult)
        {
        case LoginResult.LoginOkay:
        {
            await _accountsServiceResolver.Invoke().Update(_steamGuardAccount);
            await Toast.Make($"Login to {_steamGuardAccount.AccountName} successfully completed", ToastDuration.Long).Show();
            await Shell.Current.GoToAsync("..");
            break;
        }
        case LoginResult.TooManyFailedLogins:
            await Application.Current!.MainPage!.DisplayAlert("Login", "To many failed requests try again later", "ok");
            await Shell.Current.GoToAsync("..");
            break;
        case LoginResult.NeedCaptcha:
            CaptchaImageSource = ImageSource.FromUri(new Uri($"https://steamcommunity.com/public/captcha.php?gid={_loginAgainData.CaptchaGid}"));
            IsCaptchaBoxVisible = true;
            break;
        default:
            IsPasswordBoxEnabled = true;
            break;
        }
    }
}
