using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using SteamAuthenticatorCore.Shared.ViewModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.ComponentModel;
using SteamAuthenticatorCore.Mobile.Extensions;
using SteamAuthenticatorCore.Mobile.Pages;
using Xamarin.Forms;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Essentials;
using System;
using System.Threading;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Shared.Abstractions;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.UI.Views.Options;
using SteamAuthenticatorCore.Shared.Messages;
using SteamAuthCore.Abstractions;

namespace SteamAuthenticatorCore.Mobile.ViewModels;

public partial class TokenPageViewModel : TokenViewModelBase
{
    public TokenPageViewModel(ObservableCollection<SteamGuardAccount> accounts, IPlatformTimer platformTimer, IPlatformImplementations platformImplementations, IMessenger messenger, AccountsFileServiceResolver accountsFileServiceResolver, ISteamGuardAccountService accountService) : base(accounts, platformTimer, platformImplementations, accountService)
    {
        IsMobile = true;

        _messenger = messenger;
        _accountsFileServiceResolver = accountsFileServiceResolver;
        _accountService = accountService;
    }

    private readonly IMessenger _messenger;
    private readonly AccountsFileServiceResolver _accountsFileServiceResolver;
    private readonly ISteamGuardAccountService _accountService;

    private Frame? _longPressFrame;

    [ObservableProperty]
    private bool _isLongPressTitleViewVisible;

    public async Task UnselectLongPressFrame()
    {
        if (_longPressFrame is null) return;

        await _longPressFrame.BackgroundColorTo((Color) Application.Current.Resources[
            Application.Current.RequestedTheme == OSAppTheme.Light
                ? "SecondLightBackgroundColor"
                : "SecondDarkBackground"], 500);

        _longPressFrame = null;
    }

    [RelayCommand]
    private Task HideLongPressTitleView()
    {
        IsLongPressTitleViewVisible = false;
        return UnselectLongPressFrame();
    }

    [RelayCommand]
    private async Task Login()
    {
        var account = (SteamGuardAccount) _longPressFrame!.BindingContext;

        await Shell.Current.GoToAsync($"{nameof(LoginPage)}");
        _messenger.Send(new UpdateAccountInLoginPageMessage(account));
    }

    [RelayCommand]
    private async Task Delete()
    {
        if (!await Application.Current.MainPage.DisplayAlert("Delete account", "Are you sure?", "yes", "no"))
            return;

        var account = (SteamGuardAccount) _longPressFrame!.BindingContext;
        await _accountsFileServiceResolver.Invoke().DeleteAccount(account);

        IsLongPressTitleViewVisible = false;
        await UnselectLongPressFrame();
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
            }) ?? Enumerable.Empty<FileResult>();
        }
        catch
        {
            files = Enumerable.Empty<FileResult>();
        }

        var accountsFileService = _accountsFileServiceResolver.Invoke();

        foreach (var fileResult in files)
        {
            await using var stream = await fileResult.OpenReadAsync();
            await accountsFileService.SaveAccount(stream, fileResult.FileName);
        }
    }

    [RelayCommand]
    private async Task Copy(Frame frame)
    {
        if (string.IsNullOrEmpty(Token) || string.IsNullOrWhiteSpace(Token))
            return;

        try
        {
            HapticFeedback.Perform(HapticFeedbackType.LongPress);
        }
        catch
        {
            //
        }

        await frame.DisplaySnackBarAsync(new SnackBarOptions()
        {
            MessageOptions = new MessageOptions()
            {
                Message = "Login token copied"
            },
            Actions = Array.Empty<SnackBarActionOptions>(),
            Duration = TimeSpan.FromSeconds(2.5)
        });

        await Clipboard.SetTextAsync(Token);
    }

    [RelayCommand]
    private Task OnPress(SteamGuardAccount account)
    {
        SelectedAccount = account;

        IsLongPressTitleViewVisible = false;
        return UnselectLongPressFrame();
    }

    [RelayCommand]
    private async Task OnLongPress(Frame frame)
    {
        await UnselectLongPressFrame();

        IsLongPressTitleViewVisible = true;

        frame.BackgroundColor = (Color) Application.Current.Resources[
            Application.Current.RequestedTheme == OSAppTheme.Light
                ? "SecondLightBackgroundSelectionColor"
                : "SecondDarkBackgroundSelectionColor"];

        _longPressFrame = frame;

        try
        {
            HapticFeedback.Perform(HapticFeedbackType.LongPress);
        }
        catch
        {
            //
        }
    }
}