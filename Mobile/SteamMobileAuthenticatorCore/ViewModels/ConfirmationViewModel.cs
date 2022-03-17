﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using SteamAuthCore;
using SteamAuthenticatorCore.Shared;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Forms;
using ObservableObject = CommunityToolkit.Mvvm.ComponentModel.ObservableObject;

namespace SteamAuthenticatorCore.Mobile.ViewModels;

internal partial class ConfirmationViewModel : ObservableObject, IQueryAttributable
{
    public ConfirmationViewModel()
    {
        _accounts = DependencyService.Get<ObservableCollection<SteamGuardAccount>>();
        ConfirmationService = DependencyService.Get<BaseConfirmationService>();
    }

    private readonly ObservableCollection<SteamGuardAccount> _accounts;
    private SteamGuardAccount? _selectedAccount;

    [ObservableProperty]
    private ConfirmationAccountBase? _account;

    [ObservableProperty]
    private bool _isRefreshing;

    public BaseConfirmationService ConfirmationService { get; }


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