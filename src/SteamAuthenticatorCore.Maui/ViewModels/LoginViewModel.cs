﻿namespace SteamAuthenticatorCore.Maui.ViewModels;

public partial class LoginViewModel : MyObservableRecipient, IRecipient<UpdateAccountInLoginPageMessage>
{
    public LoginViewModel(ISteamGuardAccountService steamGuardAccountService, AccountsServiceResolver accountsServiceResolver)
    {
        _steamGuardAccountService = steamGuardAccountService;
        _accountsServiceResolver = accountsServiceResolver;
    }

    private readonly ISteamGuardAccountService _steamGuardAccountService;
    private readonly AccountsServiceResolver _accountsServiceResolver;

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

        if (_loginAgainData.LoginResult == LoginResult.LoginOkay)
        {
            await _accountsServiceResolver.Invoke().Update(_steamGuardAccount);
            await Shell.Current.GoToAsync("..");
        }

        if (_loginAgainData.LoginResult == LoginResult.TooManyFailedLogins)
        {
            await App.Current!.MainPage!.DisplayAlert("Login", "To many requests try again later", "ok");
            await Shell.Current.GoToAsync("..");
        }

        if (_loginAgainData.LoginResult == LoginResult.NeedCaptcha)
        {
            CaptchaImageSource = ImageSource.FromUri(new Uri($"https://steamcommunity.com/public/captcha.php?gid={_loginAgainData.CaptchaGid}"));
            IsCaptchaBoxVisible = true;
        }
        else
        {
            IsPasswordBoxEnabled = true;   
        }

        IsPasswordBoxEnabled = false;
    }
}
