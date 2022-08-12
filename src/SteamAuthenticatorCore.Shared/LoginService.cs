using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SteamAuthCore;
using SteamAuthCore.Manifest;

namespace SteamAuthenticatorCore.Shared;

public partial class LoginService : ObservableObject
{
    public LoginService(IPlatformImplementations platformImplementations)
    {
        _platformImplementations = platformImplementations;
        _password = string.Empty;
        _username = string.Empty;
        _additionalText = string.Empty;

        _image = _platformImplementations.CreateImage("https://www.colorcombos.com/images/colors/000000.png");
    }

    private readonly IPlatformImplementations _platformImplementations;
    private TaskCompletionSource<string> _tcs = null!;
    private SteamGuardAccount? _account;
    
    #region Properties

    public SteamGuardAccount? Account
    {
        get => _account;
        set
        {
            SetProperty(ref _account, value);

            OnPropertyChanged(nameof(IsEnabledUserNameTextBox));
            OnPropertyChanged(nameof(ButtonText));

            IsContinueButtonVisible = false;
            AdditionalText = string.Empty;
            Password = string.Empty;
            Username = value is not null ? value.AccountName : string.Empty;
            IsEmailTextBlockVisible = false;
            IsPhoneTextBlockVisible = false;
            IsCaptchaVisible = false;
            IsSmsTextBoxVisible = false;
        }
    }
    
    [ObservableProperty]
    private string _password;

    [ObservableProperty]
    private string _username;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSetUpNewVisible))]
    private bool _isContinueButtonVisible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsContinueButtonVisible))]
    private bool _isEmailTextBlockVisible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsContinueButtonVisible))]
    private bool _isPhoneTextBlockVisible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsContinueButtonVisible))]
    private bool _isCaptchaVisible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsContinueButtonVisible))]
    private bool _isSmsTextBoxVisible;

    [ObservableProperty]
    private string _additionalText;

    [ObservableProperty]
    private object _image;

    public bool IsEnabledUserNameTextBox => _account is null;

    public bool IsSetUpNewVisible => !IsContinueButtonVisible;

    public string ButtonText => _account is null ? "Set up new account" : "Login";

    #endregion

    #region Commands

    [RelayCommand]
    private void OnContinue()
    {
        if (string.IsNullOrEmpty(AdditionalText) || string.IsNullOrWhiteSpace(AdditionalText))
            return;

        if (_tcs.Task.IsCompleted)
            return;

        _tcs.SetResult(AdditionalText);
    }

    #endregion

    public async Task RefreshLogin(IManifestModelService manifestService)
    {
        if (await Login(true) is not { } session)
            return;

        Account!.Session = session;    
        await manifestService.SaveSteamGuardAccount(Account);
        await _platformImplementations.DisplayAlert("Login success");
    }

    public async Task InitLogin(IManifestModelService manifestService)
    {
        if (await Login(false) is not { } session)
            return;

        IsContinueButtonVisible = true;
        await LinkingAuthenticator(session, manifestService);
    }

    private async Task<SessionData?> Login(bool refreshLogin)
    {
        UserLogin userLogin = new(Username, Password);
        LoginResult result;

        while ((result = await userLogin.DoLogin()) != LoginResult.LoginOkay)
        {
            switch (result)
            {
                case LoginResult.NeedEmail:
                    IsEmailTextBlockVisible = true;
                    userLogin.EmailCode = await GetAdditionalText("Enter the code sent to your email");
                    continue;
                case LoginResult.NeedCaptcha:
                    IsCaptchaVisible = true;
                    userLogin.CaptchaText = await GetAdditionalText("Enter captcha");
                    IsCaptchaVisible = false;
                    continue;
                case LoginResult.Need2Fa:
                    if (!refreshLogin)
                    {
                        await _platformImplementations.DisplayAlert("This account already has a mobile authenticator linked to it.\nRemove the old authenticator from your Steam account before adding a new one.");
                        return null;
                    }

                    userLogin.TwoFactorCode = Account!.GenerateSteamGuardCode(await TimeAligner.GetSteamTimeAsync());
                    continue;
                case LoginResult.BadRsa:
                    await _platformImplementations.DisplayAlert("Error logging in: Steam returned \"BadRSA\".");
                    return null;
                case LoginResult.BadCredentials:
                    await _platformImplementations.DisplayAlert("Error logging in: Username or password was incorrect.");
                    return null;
                case LoginResult.TooManyFailedLogins:
                    await _platformImplementations.DisplayAlert("Error logging in: Too many failed logins, try again later.");
                    return null;
                case LoginResult.GeneralFailure:
                    await _platformImplementations.DisplayAlert("Error logging in: Steam returned \"GeneralFailure\".");
                    return null;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return userLogin.Session;
    }

    private async Task LinkingAuthenticator(SessionData session, IManifestModelService manifestService)
    {
        AuthenticatorLinker linker = new AuthenticatorLinker(session);
        AuthenticatorLinker.LinkResult result;

        while ((result = await linker.AddAuthenticator()) != AuthenticatorLinker.LinkResult.AwaitingFinalization)
        {
            switch (result)
            {
                case AuthenticatorLinker.LinkResult.MustProvidePhoneNumber:
                    IsPhoneTextBlockVisible = true;
                    linker.PhoneNumber = FilterPhoneNumber(await GetAdditionalText("Enter your phone number"));
                    continue;
                case AuthenticatorLinker.LinkResult.MustRemovePhoneNumber:
                    linker.PhoneNumber = null;
                    continue;
                case AuthenticatorLinker.LinkResult.MustConfirmEmail:
                    await _platformImplementations.DisplayAlert("Please check your email, and click the link Steam sent you before continuing.");
                    continue;
                case AuthenticatorLinker.LinkResult.GeneralFailure:
                    await _platformImplementations.DisplayAlert("Error adding your phone number. Steam returned \"GeneralFailure\".");
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        await OnLinkerAwaitingFinalization(linker, manifestService);
    }

    private async Task OnLinkerAwaitingFinalization(AuthenticatorLinker linker, IManifestModelService manifestService)
    {
        await _platformImplementations.DisplayAlert($"The Mobile Authenticator has not yet been linked. Before finalizing the authenticator, please write down your revocation code: {linker.LinkedAccount.RevocationCode}");

        while (true)
        {
            IsSmsTextBoxVisible = true;
            AdditionalText = string.Empty;
            var result = await linker.FinalizeAddAuthenticator(await GetAdditionalText("Please input the SMS code sent to your phone."));
            switch (result)
            {
                case AuthenticatorLinker.FinalizeResult.BadSmsCode:
                    continue;
                case AuthenticatorLinker.FinalizeResult.UnableToGenerateCorrectCodes:
                    await _platformImplementations.DisplayAlert($"Unable to generate the proper codes to finalize this authenticator. The authenticator should not have been linked. In the off-chance it was, please write down your revocation code, as this is the last chance to see it: {linker.LinkedAccount.RevocationCode}");
                    return;
                case AuthenticatorLinker.FinalizeResult.GeneralFailure:
                    await _platformImplementations.DisplayAlert($"Unable to finalize this authenticator. The authenticator should not have been linked. In the off-chance it was, please write down your revocation code, as this is the last chance to see it: {linker.LinkedAccount.RevocationCode}");
                    return;
                case AuthenticatorLinker.FinalizeResult.Success:
                    await manifestService.SaveSteamGuardAccount(linker.LinkedAccount);
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private async Task<string> GetAdditionalText(string message)
    {
        _tcs = new TaskCompletionSource<string>();

        await _platformImplementations.DisplayAlert(message);
        return await _tcs.Task;
    }

    private static string FilterPhoneNumber(string phoneNumber)
    {
        return phoneNumber.Replace("-", "").Replace("(", "").Replace(")", "");
    }
}