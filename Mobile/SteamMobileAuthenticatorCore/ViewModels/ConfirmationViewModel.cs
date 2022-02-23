using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Input;
using SteamAuthCore;
using SteamAuthenticatorCore.Shared;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;

namespace SteamAuthenticatorCore.Mobile.ViewModels;

internal class ConfirmationViewModel : BaseViewModel, IQueryAttributable
{
    public ConfirmationViewModel()
    {
        _accounts = DependencyService.Get<ObservableCollection<SteamGuardAccount>>();
        ConfirmationService = DependencyService.Get<BaseConfirmationService>();
    }

    private readonly ObservableCollection<SteamGuardAccount> _accounts;
    private SteamGuardAccount? _selectedAccount;
    private bool _isRefreshing;
    private ConfirmationAccountBase? _account;

    public BaseConfirmationService ConfirmationService { get; }

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
    }

    public ConfirmationAccountBase? Account
    {
        get => _account;
        set => SetProperty(ref _account, value);
    }


    public void ApplyQueryAttributes(IDictionary<string, string> query)
    {
        var id= HttpUtility.UrlDecode(query["id"]);
        _selectedAccount = _accounts[Convert.ToInt32(id)];

        IsRefreshing = true;
    }

    public ICommand RefreshCommand => new AsyncCommand( async () =>
    {
        await RefreshConfirmations();
    });

    private async Task RefreshConfirmations()
    {
        if (_selectedAccount != null)
            Account = await ConfirmationService.CreateConfirmationAccount(_selectedAccount);

        IsRefreshing = false;
    }
}