using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SteamAuthCore;
using SteamAuthenticatorCore.Mobile.Extensions;
using SteamAuthenticatorCore.Shared;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SteamAuthenticatorCore.Mobile.ViewModels;

internal partial class ConfirmationViewModel : ObservableObject, IQueryAttributable
{
    public ConfirmationViewModel(BaseConfirmationService confirmationService)
    {
        ConfirmationService = confirmationService;
        SelectedItems = new ObservableCollection<(Frame, ConfirmationModel)>();
    }

    [ObservableProperty]
    private ConfirmationAccountBase _account = null!;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private bool _isCountTitleViewVisible;

    public BaseConfirmationService ConfirmationService { get; }

    public ObservableCollection<(Frame, ConfirmationModel)> SelectedItems { get; }


    public void ApplyQueryAttributes(IDictionary<string, string> query)
    {
        var id= HttpUtility.UrlDecode(query["id"]);
        Account = ConfirmationService.Accounts[Convert.ToInt32(id)];
    }

    [ICommand]
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

    [ICommand]
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

    [ICommand]
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

    [ICommand]
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

    [ICommand]
    private Task CancelSelected()
    {
        switch (SelectedItems.Count)
        {
            case 0:
                return Task.CompletedTask;
            case 1:
                SendConfirmation(SteamGuardAccount.Confirmation.Deny);
                break;
            default:
                SendConfirmations(SteamGuardAccount.Confirmation.Deny);
                break;
        }

        return HideCountTitleView();
    }

    private void SendConfirmation(SteamGuardAccount.Confirmation confirmation)
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
        Account!.SendConfirmation(model.Item2, confirmation);
    }

    private void SendConfirmations(SteamGuardAccount.Confirmation confirmation)
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

        Account?.SendConfirmations(items, confirmation);
    }
}