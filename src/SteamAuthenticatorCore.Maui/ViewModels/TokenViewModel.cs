﻿using SteamAuthCore.Models;

namespace SteamAuthenticatorCore.Maui.ViewModels;

public sealed partial class TokenViewModel : MyObservableRecipient, IAsyncDisposable
{
    public TokenViewModel(AccountsServiceResolver accountsFileServiceResolver, ISteamGuardAccountService steamGuardAccountService, IBackgroundTimerFactory backgroundTimerFactory)
    {
        _steamGuardAccountService = steamGuardAccountService;
        _accountsService = accountsFileServiceResolver.Invoke();
        _token = TokenPlaceHolder;
        _appSettings = AppSettings.Current;

        _backgroundTimer = backgroundTimerFactory.StartNewTimer(TimeSpan.FromSeconds(1), OnTimer);
    }

    private const string TokenPlaceHolder = "Token";

    private readonly ISteamGuardAccountService _steamGuardAccountService;
    private readonly IBackgroundTimer _backgroundTimer;
    private readonly IAccountsService _accountsService;
    private readonly AppSettings _appSettings;
    private IReadOnlyList<SteamGuardAccount> _accounts = Array.Empty<SteamGuardAccount>();

    private Int64 _currentSteamChunk;
    private VisualElement? _longPressView;
    private bool _pressed;

    public ObservableCollection<SteamGuardAccount> FilteredAccounts { get; } = new();

    [ObservableProperty]
    private string _token;

    [ObservableProperty]
    private bool _isLongPressTitleViewVisible;

    [ObservableProperty]
    private SteamGuardAccount? _selectedAccount;

    [ObservableProperty]
    private double _tokenProgressBar;

    [ObservableProperty]
    private string _searchBoxText = string.Empty;

    protected override async void OnActivated()
    {
        base.OnActivated();

        await RefreshAccounts().ConfigureAwait(false);

        Shell.Current.Navigating += ShellOnNavigating;
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        HideLongPressTitleView();
        Shell.Current.Navigating -= ShellOnNavigating;
    }

    public async ValueTask DisposeAsync()
    {
        await _backgroundTimer.DisposeAsync().ConfigureAwait(false);
    }

    #region Commands

    [RelayCommand]
    private async Task Import()
    {
        IEnumerable<FileResult> files;
        if (Application.Current!.MainPage is null) return;

        try
        {
            files = await FilePicker.PickMultipleAsync(new PickOptions()
            {
                PickerTitle = "Select maFile",
                /*FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>()
                {
                    { DevicePlatform.Android, new[] { "application/octet-stream", } },
                })*/
            }) ?? Array.Empty<FileResult>();
        }
        catch
        {
            files = Enumerable.Empty<FileResult>();
        }

        foreach (var fileResult in files)
        {
            try
            {
                await using var stream = await fileResult.OpenReadAsync();

                if (!await _accountsService.Save(stream, fileResult.FileName))
                {
                    await Application.Current.MainPage!.DisplayAlert("Error",
                        $"Failed to deserialize file or it was already added - {fileResult.FileName}", "Ok");
                }
            }
            catch (Exception e)
            {
                await Application.Current.MainPage!.DisplayAlert(
                    $"Exception while adding - {fileResult.FileName} file", e.ToString(), "Ok");
            }
        }

        await RefreshAccounts();
    }

    [RelayCommand]
    private async Task Login()
    {
        var account = (SteamGuardAccount) _longPressView!.BindingContext;
        
        await Shell.Current.GoToAsync($"{nameof(LoginPage)}");

        Messenger.Send(new UpdateAccountInLoginPageMessage(account));
    }

    [RelayCommand]
    private async Task Delete()
    {
        var account = (SteamGuardAccount) _longPressView!.BindingContext;

        var message = string.Format(_appSettings.LocalizationProvider[LocalizationMessage.DeletingAccountContentMessage],
            account.AccountName);

        if (!await Application.Current!.MainPage!.DisplayAlert(_appSettings.LocalizationProvider[LocalizationMessage.DeletingAccountMessage], message, _appSettings.LocalizationProvider[LocalizationMessage.YesMessage], _appSettings.LocalizationProvider[LocalizationMessage.NoMessage]))
            return;

        await _accountsService.Delete(account);
        HideLongPressTitleView();

        await RefreshAccounts();
    }

    [RelayCommand]
    private async Task Copy(string token)
    {
        if (string.IsNullOrWhiteSpace(token) || token == TokenPlaceHolder)
            return;

        try
        {
            HapticFeedback.Perform(HapticFeedbackType.Click);
        }
        catch
        {
            //
        }

        await Clipboard.SetTextAsync(Token);


        var toast = Toast.Make(AppSettings.Current.LocalizationProvider[LocalizationMessage.CopiedMessage], ToastDuration.Short, 16);
        await toast.Show();
    }

    [RelayCommand]
    private void OnPress(SteamGuardAccount account)
    {
        if (_pressed)
        {
            _pressed = false;
            return;
        }

        SelectedAccount = account;
        HideLongPressTitleView();
    }

    [RelayCommand]
    private void OnLongPress(VisualElement view)
    {
        HideLongPressTitleView();

        IsLongPressTitleViewVisible = true;

        view.Opacity = 0.65;
        _longPressView = view;
        _pressed = true;

        try
        {
            HapticFeedback.Perform(HapticFeedbackType.LongPress);
        }
        catch
        {
            //
        }
    }

    [RelayCommand]
    private void HideLongPressTitleView()
    {
        if (_longPressView is null) 
            return;

        IsLongPressTitleViewVisible = false;
        _longPressView.Opacity = 1;
        _longPressView = null;
    }

    #endregion

    private void OnTimer(CancellationToken obj)
    {
        if (SelectedAccount is null)
            return;

        var steamTime = ITimeAligner.SteamTime;
        _currentSteamChunk = steamTime / 30L;
        var secondsUntilChange = (int)(steamTime - _currentSteamChunk * 30L);

        if (SelectedAccount.GenerateSteamGuardCode() is { } token)
            Token = token;

        TokenProgressBar = 30 - secondsUntilChange;
        TokenProgressBar /= 30;
    }

    private async ValueTask RefreshAccounts()
    {
        _accounts = await _accountsService.GetAll();
        OnSearchBoxTextChanged(null, SearchBoxText);
    }

    partial void OnSearchBoxTextChanged(string? oldValue, string newValue)
    {
        if (string.IsNullOrWhiteSpace(newValue))
        {
            FilteredAccounts.Clear();

            foreach (var account in _accounts)
                FilteredAccounts.Add(account);

            return;
        }

        var suitableItems = new List<SteamGuardAccount>();
        var splitText = newValue.ToLower().Split(' ');

        foreach (var account in _accounts)
        {
            var itemText = account.AccountName;

            var found = splitText.All(key=> itemText.ToLower().Contains(key));

            if (found)
                suitableItems.Add(account);
        }

        FilteredAccounts.Clear();

        foreach (var account in suitableItems)
            FilteredAccounts.Add(account);
    }

    private void ShellOnNavigating(object? sender, ShellNavigatingEventArgs e)
    {
        if (e.Current.Location.OriginalString.Length != nameof(TokenPage).Length + 2)
            return;

        if (IsLongPressTitleViewVisible && e.Target.Location.OriginalString.Contains(nameof(LoginPage)))
            return;

        if (!IsLongPressTitleViewVisible)
            return;

        e.Cancel();

        HideLongPressTitleView();
    }
}
