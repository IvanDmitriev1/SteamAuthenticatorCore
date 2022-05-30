using System;
using System.Collections.Generic;
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
        _selectedItems = new List<ConfirmationModel>();
    }

    private readonly List<ConfirmationModel> _selectedItems;

    [ObservableProperty]
    private ConfirmationAccountBase _account = null!;

    [ObservableProperty]
    private bool _isRefreshing;

    public BaseConfirmationService ConfirmationService { get; }


    public void ApplyQueryAttributes(IDictionary<string, string> query)
    {
        var id= HttpUtility.UrlDecode(query["id"]);
        Account = ConfirmationService.Accounts[Convert.ToInt32(id)];
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
    private void ConfirmSelected()
    {
        switch (_selectedItems.Count)
        {
            case 0:
                return;
            case 1:
                SendConfirmation(SteamGuardAccount.Confirmation.Allow);
                return;
            default:
                SendConfirmations(SteamGuardAccount.Confirmation.Allow);
                break;
        }
    }

    [ICommand]
    private void CancelSelected()
    {
        switch (_selectedItems.Count)
        {
            case 0:
                return;
            case 1:
                SendConfirmation(SteamGuardAccount.Confirmation.Deny);
                return;
            default:
                SendConfirmations(SteamGuardAccount.Confirmation.Deny);
                break;
        }
    }

    [ICommand]
    private Task OnElementTouch(Frame frame)
    {
        var confirmationModel = (ConfirmationModel) frame.BindingContext;

        if (_selectedItems.Contains(confirmationModel))
        {
            _selectedItems.Remove(confirmationModel);

            return frame.BackgroundColorTo((Color) Application.Current.Resources[
                Application.Current.RequestedTheme == OSAppTheme.Light
                    ? "SecondLightBackgroundColor"
                    : "SecondDarkBackground"], 500);
        }

        _selectedItems.Add(confirmationModel);

        return frame.BackgroundColorTo((Color) Application.Current.Resources[
            Application.Current.RequestedTheme == OSAppTheme.Light
                ? "SecondLightBackgroundSelectionColor"
                : "SecondDarkBackgroundSelectionColor"], 500);
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

        var model = _selectedItems[0];
        Account!.SendConfirmation(model, confirmation);
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

        Account?.SendConfirmations(_selectedItems, confirmation);
    }
}