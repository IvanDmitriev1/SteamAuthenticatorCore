using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.MobileMaui.Pages;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.Messages;
using SteamAuthenticatorCore.Shared.ViewModel;

namespace SteamAuthenticatorCore.MobileMaui.ViewModels;

public partial class TokenViewModel : TokenViewModelBase, IDisposable
{
    public TokenViewModel(ObservableCollection<SteamGuardAccount> accounts, IPlatformTimer platformTimer, IMessenger messenger, AccountsFileServiceResolver accountsFileServiceResolver, ISteamGuardAccountService accountService) : base(accounts, platformTimer)
    {
        _messenger = messenger;
        _accountsFileServiceResolver = accountsFileServiceResolver;
        _accountService = accountService;

        Token = "Login token";
        IsMobile = true;

        Shell.Current.Navigating += CurrentOnNavigating;
    }

    private async void CurrentOnNavigating(object? sender, ShellNavigatingEventArgs e)
    {
        if (e.Current.Location.OriginalString.Length != nameof(TokenPage).Length + 2)
            return;

        if (_longPressView is null || e.Target.Location.OriginalString.Contains(nameof(LoginPage)))
            return;

        e.Cancel();

        await HideLongPressTitleView();
    }

    private readonly IMessenger _messenger;
    private readonly AccountsFileServiceResolver _accountsFileServiceResolver;
    private readonly ISteamGuardAccountService _accountService;

    private View? _longPressView;
    private bool _pressed;

    [ObservableProperty]
    private bool _isLongPressTitleViewVisible;

    public void Dispose()
    {
        _longPressView = null;
        Shell.Current.Navigating -= CurrentOnNavigating;
    }

    [RelayCommand]
    private async Task Import()
    {
        IEnumerable<FileResult> files;

        try
        {
            files = await FilePicker.PickMultipleAsync(new PickOptions()
            {
                PickerTitle = "Select maFile"
            }).ConfigureAwait(false) ?? Enumerable.Empty<FileResult>();
        }
        catch
        {
            files = Enumerable.Empty<FileResult>();
        }

        var accountsFileService = _accountsFileServiceResolver.Invoke();

        foreach (var fileResult in files)
        {
            await using var stream = await fileResult.OpenReadAsync().ConfigureAwait(false);
            await accountsFileService.SaveAccount(stream, fileResult.FileName).ConfigureAwait(false);
        }
    }

    [RelayCommand]
    private async Task Login()
    {
        var account = (SteamGuardAccount) _longPressView!.BindingContext;

        await Shell.Current.GoToAsync($"{nameof(LoginPage)}");
        _messenger.Send(new UpdateAccountInLoginPageMessage(account));
    }

    [RelayCommand]
    private async Task ForceRefreshSession()
    {
        var account = (SteamGuardAccount) _longPressView!.BindingContext;

        if (await _accountService.RefreshSession(account, CancellationToken.None))
            await Application.Current!.MainPage!.DisplayAlert("Refresh session", "Session has been refreshed", "Ok");
        else
            await Application.Current!.MainPage!.DisplayAlert("Refresh session", "Failed to refresh session", "Ok");
    }

    [RelayCommand]
    private async Task Delete()
    {
        if (!await Application.Current!.MainPage!.DisplayAlert("Delete account", "Are you sure?", "yes", "no"))
            return;

        var account = (SteamGuardAccount) _longPressView!.BindingContext;
        await _accountsFileServiceResolver.Invoke().DeleteAccount(account);

        IsLongPressTitleViewVisible = false;
        await UnselectLongPressFrame();
    }

    [RelayCommand]
    private async Task Copy(string token)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrWhiteSpace(token) || token == "Login token")
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

        var toast = Toast.Make("Login token copied");
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
    private async Task OnLongPress(View view)
    {
        await UnselectLongPressFrame();

        IsLongPressTitleViewVisible = true;

        var value = Application.Current!.RequestedTheme == AppTheme.Light
            ? "SecondLightBackgroundSelectionColor"
            : "SecondDarkBackgroundSelectionColor";

        Application.Current!.Resources.TryGetValue(value, out var color);

        view.BackgroundColor = (Color)color!;
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

    private async Task UnselectLongPressFrame()
    {
        if (_longPressView is null) 
            return;

        var value = Application.Current!.RequestedTheme == AppTheme.Light
            ? "SecondLightBackgroundColor"
            : "SecondDarkBackground";

        Application.Current!.Resources.TryGetValue(value, out var color);

        await _longPressView.BackgroundColorTo((Color)color!, 16, 150);
        _longPressView = null;
    }
}
