using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Mobile.Extensions;
using SteamAuthenticatorCore.Mobile.Pages;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.Messages;
using SteamAuthenticatorCore.Shared.ViewModel;

namespace SteamAuthenticatorCore.Mobile.ViewModels;

public partial class TokenViewModel : TokenViewModelBase, IDisposable
{
    public TokenViewModel(ObservableCollection<SteamGuardAccount> accounts, IValueTaskTimer valueTaskTimer, IPlatformImplementations platformImplementations, ISteamGuardAccountService accountService, AccountsFileServiceResolver accountsFileServiceResolver, IMessenger messenger) : base(accounts, valueTaskTimer, platformImplementations, accountService, accountsFileServiceResolver)
    {
        _messenger = messenger;
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

    private VisualElement? _longPressView;
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

        var accountsFileService = AccountsFileServiceResolver.Invoke();

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
    private Task ForceRefreshSession()
    {
        var account = (SteamGuardAccount) _longPressView!.BindingContext;
        return RefreshAccountsSession(account);
    }

    [RelayCommand]
    private async Task Delete()
    {
        var account = (SteamGuardAccount) _longPressView!.BindingContext;
        await DeleteAccount(account);

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
    private async Task OnLongPress(VisualElement view)
    {
        await UnselectLongPressFrame();

        IsLongPressTitleViewVisible = true;

        await view.ChangeBackgroundColorToWithColorsCollection("SecondBackgroundSelectionColor");
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

        await _longPressView.ChangeBackgroundColorToWithColorsCollection("SecondBackgroundColor"); 
        _longPressView = null;
    }
}
