using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Web;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SteamAuthCore;
using SteamAuthenticatorCore.Mobile.Extensions;
using SteamAuthenticatorCore.Shared.Models;
using SteamAuthenticatorCore.Shared.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SteamAuthenticatorCore.Mobile.ViewModels;

internal partial class ConfirmationViewModel : ObservableObject, IQueryAttributable
{
    public ConfirmationViewModel(ConfirmationServiceBase confirmationServiceBase)
    {
        ConfirmationService = confirmationServiceBase;
        SelectedItems = new ObservableCollection<(Frame, ConfirmationModel)>();
    }

    [ObservableProperty]
    private ConfirmationAccountModelBase _account = null!;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private bool _isCountTitleViewVisible;

    public ConfirmationServiceBase ConfirmationService { get; }

    public ObservableCollection<(Frame, ConfirmationModel)> SelectedItems { get; }


    public void ApplyQueryAttributes(IDictionary<string, string> query)
    {
        var id= HttpUtility.UrlDecode(query["id"]);
        Account = ConfirmationService.Accounts[Convert.ToInt32(id)];
    }

    [RelayCommand]
    private async Task HideCountTitleView()
    {
        var tasks = new Task[SelectedItems.Count];

        for (var i = 0; i < SelectedItems.Count; i++)
        {
            var frame = SelectedItems[i].Item1;
            tasks[i] = frame.BackgroundColorTo((Color) Application.Current.Resources[
                Application.Current.RequestedTheme == OSAppTheme.Light
                    ? "SecondLightBackgroundColor"
                    : "SecondDarkBackground"], 500);
        }

        IsCountTitleViewVisible = false;
        await Task.WhenAll(tasks);
        SelectedItems.Clear();
    }

    [RelayCommand]
    private Task OnElementTouch(Frame frame)
    {
        var item = (frame, (ConfirmationModel) frame.BindingContext);

        if (SelectedItems.Contains(item))
        {
            SelectedItems.Remove(item);

            if (SelectedItems.Count == 0)
                IsCountTitleViewVisible = false;

            return frame.BackgroundColorTo((Color) Application.Current.Resources[
                Application.Current.RequestedTheme == OSAppTheme.Light
                    ? "SecondLightBackgroundColor"
                    : "SecondDarkBackground"], 500);
        }

        SelectedItems.Add(item);
        IsCountTitleViewVisible = true;

        return frame.BackgroundColorTo((Color) Application.Current.Resources[
            Application.Current.RequestedTheme == OSAppTheme.Light
                ? "SecondLightBackgroundSelectionColor"
                : "SecondDarkBackgroundSelectionColor"], 500);
    }

    [RelayCommand]
    private async Task RefreshConfirmations()
    {
        try
        {
            HapticFeedback.Perform(HapticFeedbackType.LongPress);
        }
        catch
        {
            //
        }

        

        await _account.CheckConfirmations();
        IsRefreshing = false;
    }

    [RelayCommand]
    private Task ConfirmSelected()
    {
        switch (SelectedItems.Count)
        {
            case 0:
                return Task.CompletedTask;
            case 1:
                SendConfirmation(SteamGuardAccount.Confirmation.Allow);
                break;
            default:
                SendConfirmations(SteamGuardAccount.Confirmation.Allow);
                break;
        }

        return HideCountTitleView();
    }

    [RelayCommand]
    private async Task CancelSelected()
    {
        switch (SelectedItems.Count)
        {
            case 0:
                return;
            case 1:
                await SendConfirmation(SteamGuardAccount.Confirmation.Deny);
                break;
            default:
                await SendConfirmations(SteamGuardAccount.Confirmation.Deny);
                break;
        }

        await HideCountTitleView();
    }

    private Task SendConfirmation(SteamGuardAccount.Confirmation confirmation)
    {
        try
        {
            HapticFeedback.Perform(HapticFeedbackType.Click);
        }
        catch
        {
            //
        }

        var model = SelectedItems[0];
        return Account!.SendConfirmation(model.Item2, confirmation);
    }

    private Task SendConfirmations(SteamGuardAccount.Confirmation confirmation)
    {
        try
        {
            HapticFeedback.Perform(HapticFeedbackType.Click);
        }
        catch
        {
            //
        }

        var items = new ConfirmationModel[SelectedItems.Count];
        for (var i = 0; i < SelectedItems.Count; i++)
            items[i] = SelectedItems[i].Item2;

        return Account?.SendConfirmations(items, confirmation)!;
    }
}