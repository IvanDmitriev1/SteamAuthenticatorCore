using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SteamAuthCore;
using SteamAuthCore.Manifest;
using SteamAuthenticatorCore.Mobile.Extensions;
using SteamAuthenticatorCore.Mobile.Pages;
using SteamAuthenticatorCore.Shared;
using Xamarin.CommunityToolkit.Extensions;
using Xamarin.CommunityToolkit.UI.Views.Options;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SteamAuthenticatorCore.Mobile.ViewModels;

public partial class TokenPageViewModel : ObservableObject
{
    public TokenPageViewModel(IManifestModelService manifestModelService, ObservableCollection<SteamGuardAccount> accounts, TokenService tokenService)
    {
        _manifestModelService = manifestModelService;
        Accounts = accounts;
        TokenService = tokenService;
    }

    private readonly IManifestModelService _manifestModelService;

    #region Properties

    private SteamGuardAccount? _selectedSteamGuardAccount;

    public ObservableCollection<SteamGuardAccount> Accounts { get; }

    public SteamGuardAccount? SelectedSteamGuardAccount
    {
        get => _selectedSteamGuardAccount;
        set
        {
            SetProperty(ref _selectedSteamGuardAccount, value);
            TokenService.SelectedAccount = value;
        }
    }

    public TokenService TokenService { get; }

    [ObservableProperty]
    private bool _isLongPressTitleViewVisible;

    private Frame? _longPressFrame;

    #endregion

    #region Commands

    [ICommand]
    private async Task Import()
    {
        IEnumerable<FileResult> files;

        try
        {
            files = await FilePicker.PickMultipleAsync(new PickOptions()
            {
                PickerTitle = "Select maFile"
            }) ?? Array.Empty<FileResult>();
        }
        catch
        {
            files = Array.Empty<FileResult>();
        }

        foreach (var file in files)
        {
            try
            {
                await using var stream = await file.OpenReadAsync();
                if (await _manifestModelService.AddSteamGuardAccount(stream, file.FileName) is { } account)
                    Accounts.Add(account);
            }
            catch
            {
                await Application.Current.MainPage.DisplayAlert("Import account", $"failed to import {file.FileName}, maybe your file is corrupted?", "Ok");
            }
        }
    }

    [ICommand]
    private async Task Copy(Frame frame)
    {
        if (string.IsNullOrEmpty(TokenService.Token) || string.IsNullOrWhiteSpace(TokenService.Token)) return;

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
        await Clipboard.SetTextAsync(TokenService.Token);
    }

    [ICommand]
    private Task OnConfirmations()
    {
        return Task.CompletedTask;

        /*var account = (SteamGuardAccount) _longPressFrame!.BindingContext;

        SelectedSteamGuardAccount = Accounts[Accounts.IndexOf(account)];
        await Shell.Current.GoToAsync($"{nameof(ConfirmationsPage)}?id={Accounts.IndexOf(account)}");*/
    }

    [ICommand]
    private async Task Delete()
    {
        var account = (SteamGuardAccount) _longPressFrame!.BindingContext;

        if (await Application.Current.MainPage.DisplayAlert("Delete account", "Are you sure?", "yes", "no"))
        {
            await _manifestModelService.DeleteSteamGuardAccount(account);
            Accounts.Remove(account);

            IsLongPressTitleViewVisible = false;
            await UnselectLongPressFrame();

            TokenService.SelectedAccount = null;
        }
    }

    [ICommand]
    private async Task Login()
    {
        var account = (SteamGuardAccount) _longPressFrame!.BindingContext;

        await Shell.Current.GoToAsync($"{nameof(LoginPage)}?id={Accounts.IndexOf(account)}");
    }

    [ICommand]
    private async Task ForceRefreshSession()
    {
        var account = (SteamGuardAccount) _longPressFrame!.BindingContext;

        if (await account.RefreshSessionAsync())
            await Application.Current.MainPage.DisplayAlert("Refresh session", "the session has been refreshed", "Ok");
    }

    [ICommand]
    private Task OnPress(SteamGuardAccount account)
    {
        SelectedSteamGuardAccount = account;
        IsLongPressTitleViewVisible = false;
        return UnselectLongPressFrame();
    }

    [ICommand]
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

    [ICommand]
    private Task HideLongPressTitleView()
    {
        IsLongPressTitleViewVisible = false;
        return UnselectLongPressFrame();
    }

   #endregion

   public async Task UnselectLongPressFrame()
   {
       if (_longPressFrame is null) return;

       await _longPressFrame.BackgroundColorTo((Color) Application.Current.Resources[
           Application.Current.RequestedTheme == OSAppTheme.Light
               ? "SecondLightBackgroundColor"
               : "SecondDarkBackground"], 500);

       _longPressFrame = null;
   }
}