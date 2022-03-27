using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SteamAuthCore;
using SteamAuthenticatorCore.Shared;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SteamAuthenticatorCore.Mobile.ViewModels;

internal partial class ConfirmationViewModel : ObservableObject, IQueryAttributable
{
    public ConfirmationViewModel()
    {
        _accounts = DependencyService.Get<ObservableCollection<SteamGuardAccount>>();
        ConfirmationService = DependencyService.Get<BaseConfirmationService>();
        SelectedCollection = new ObservableCollection<object>();
    }

    private readonly ObservableCollection<SteamGuardAccount> _accounts;
    private SteamGuardAccount? _selectedAccount;

    [ObservableProperty]
    private ConfirmationAccountBase? _account;

    [ObservableProperty]
    private bool _isRefreshing;

    public ObservableCollection<object> SelectedCollection { get; }

    public BaseConfirmationService ConfirmationService { get; }


    public void ApplyQueryAttributes(IDictionary<string, string> query)
    {
        var id= HttpUtility.UrlDecode(query["id"]);
        _selectedAccount = _accounts[Convert.ToInt32(id)];

        IsRefreshing = true;
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

        if (_selectedAccount != null)
            Account = await ConfirmationService.CreateConfirmationAccount(_selectedAccount);

        IsRefreshing = false;
    }

    [ICommand]
    private void ConfirmSelected()
    {
        if (SelectedCollection.Count == 1)
            SendConfirmation(SteamGuardAccount.Confirmation.Allow);

        SendConfirmations(SteamGuardAccount.Confirmation.Allow);
    }

    [ICommand]
    private void CancelSelected()
    {
        if (SelectedCollection.Count == 1)
            SendConfirmation(SteamGuardAccount.Confirmation.Deny);

        SendConfirmations(SteamGuardAccount.Confirmation.Deny);
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

        var model = (ConfirmationModel) SelectedCollection[0];
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


        var model = SelectedCollection.Cast<ConfirmationModel>();
        Account?.SendConfirmations(ref model, confirmation);
    }
}