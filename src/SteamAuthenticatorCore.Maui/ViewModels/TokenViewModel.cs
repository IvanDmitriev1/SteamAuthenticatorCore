using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Mobile.Pages;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.Messages;
using System.Collections.ObjectModel;

namespace SteamAuthenticatorCore.Mobile.ViewModels;

public partial class TokenViewModel : ObservableRecipient
{
    public TokenViewModel(AccountsServiceResolver accountsFileServiceResolver, ISteamGuardAccountService steamGuardAccountService, ITimer timer)
    {
        _steamGuardAccountService = steamGuardAccountService;
        _timer = timer;
        _accountsService = accountsFileServiceResolver.Invoke();
        _token = "Token";
    }

    private readonly ISteamGuardAccountService _steamGuardAccountService;
    private readonly ITimer _timer;
    private readonly IAccountsService _accountsService;
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
        await _timer.StartOrRestart(TimeSpan.FromSeconds(1), OnTimer).ConfigureAwait(false);

        Shell.Current.Navigating += ShellOnNavigating;
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        _longPressView = null;
        Shell.Current.Navigating -= ShellOnNavigating;
    }

    #region Commands

    [RelayCommand]
    private async Task Import()
    {
        IEnumerable<FileResult> files;

        try
        {
            files = await FilePicker.PickMultipleAsync(new PickOptions()
            {
                PickerTitle = "Select maFile"
            }).ConfigureAwait(false) ?? Array.Empty<FileResult>();
        }
        catch
        {
            files = Enumerable.Empty<FileResult>();
        }

        foreach (var fileResult in files)
        {
            await using var stream = await fileResult.OpenReadAsync().ConfigureAwait(false);
            await _accountsService.Save(stream, fileResult.FileName).ConfigureAwait(false);
        }

        await RefreshAccounts().ConfigureAwait(false);
    }

    [RelayCommand]
    private async Task Login()
    {
        var account = (SteamGuardAccount) _longPressView!.BindingContext;

        await Shell.Current.GoToAsync($"{nameof(LoginPage)}");

        Messenger.Send(new UpdateAccountInLoginPageMessage(account));
    }

    [RelayCommand]
    private async Task ForceRefreshSession()
    {
        var account = (SteamGuardAccount) _longPressView!.BindingContext;

        if (!await _steamGuardAccountService.RefreshSession(account, CancellationToken.None))
        {
            await Application.Current!.MainPage!.DisplayAlert("Session refresh", "Failed to refresh session", "Ok");
            return;
        }

        await Application.Current!.MainPage!.DisplayAlert("Session refresh", "Session has been refreshed", "Ok");
    }

    [RelayCommand]
    private async Task Delete()
    {
        var account = (SteamGuardAccount) _longPressView!.BindingContext;

        if (!await Microsoft.Maui.Controls.Application.Current!.MainPage!.DisplayAlert("Delete account", $"Are you sure what you want to delete {account.AccountName}?", "Yes", "No"))
            return;

        await _accountsService.Delete(account);

        IsLongPressTitleViewVisible = false;
        await UnselectLongPressFrame();

        await RefreshAccounts().ConfigureAwait(false);
    }

    [RelayCommand]
    private async Task Copy(string token)
    {
        if (string.IsNullOrWhiteSpace(token) || token == "Token")
            return;

        try
        {
            HapticFeedback.Perform(HapticFeedbackType.LongPress);
        }
        catch
        {
            //
        }

        await Clipboard.SetTextAsync(Token).ConfigureAwait(false);
        var toast = Toast.Make("Copied");
        await toast.Show().ConfigureAwait(false);
    }

    [RelayCommand]
    private Task OnPress(SteamGuardAccount account)
    {
        if (_pressed)
        {
            _pressed = false;
            return Task.CompletedTask;
        }

        SelectedAccount = account;
        IsLongPressTitleViewVisible = false;
        return UnselectLongPressFrame();
    }

    [RelayCommand]
    private async Task OnLongPress(VisualElement view)
    {
        await UnselectLongPressFrame();

        IsLongPressTitleViewVisible = true;

        //await view.ChangeBackgroundColorToWithColorsCollection("SecondBackgroundSelectionColor");

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
    private Task HideLongPressTitleView()
    {
        IsLongPressTitleViewVisible = false;
        return UnselectLongPressFrame();
    }

    #endregion

    private async Task UnselectLongPressFrame()
    {
        if (_longPressView is null) 
            return;

        //await _longPressView.ChangeBackgroundColorToWithColorsCollection("SecondBackgroundColor"); 
        _longPressView = null;
    }

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
        _accounts = await _accountsService.GetAll().ConfigureAwait(false);
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

    private async void ShellOnNavigating(object? sender, ShellNavigatingEventArgs e)
    {
        if (e.Current.Location.OriginalString.Length != nameof(TokenPage).Length + 2)
            return;

        if (IsLongPressTitleViewVisible && e.Target.Location.OriginalString.Contains(nameof(LoginPage)))
            return;

        if (!IsLongPressTitleViewVisible)
            return;

        e.Cancel();

        await HideLongPressTitleView().ConfigureAwait(false);
    }
}
