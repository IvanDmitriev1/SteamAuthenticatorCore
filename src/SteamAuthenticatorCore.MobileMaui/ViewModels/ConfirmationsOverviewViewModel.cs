﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAuthenticatorCore.MobileMaui.Pages;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.Messages;

namespace SteamAuthenticatorCore.MobileMaui.ViewModels;

public sealed partial class ConfirmationsOverviewViewModel : ObservableObject
{
    public ConfirmationsOverviewViewModel(IConfirmationService confirmationService, IMessenger messenger)
    {
        _messenger = messenger;
        ConfirmationService = confirmationService;
    }

    private readonly IMessenger _messenger;
    private bool _needRefresh;

    [ObservableProperty]
    private bool _isRefreshing;

    public IConfirmationService ConfirmationService { get; }

    [RelayCommand]
    private void OnAppearing()
    {
        if (_needRefresh)
        {
            _needRefresh = false;
            IsRefreshing = true;
        }

        if (ConfirmationService.ConfirmationViewModels.Count > 0)
            return;

        _needRefresh = true;
        IsRefreshing = true;
        _needRefresh = false;
    }

    [RelayCommand]
    private async Task Refresh()
    {
        if (!_needRefresh)
        {
            try
            {
                HapticFeedback.Perform(HapticFeedbackType.LongPress);
            }
            catch
            {
                //
            }
        }

        await ConfirmationService.CheckConfirmations();
        IsRefreshing = false;
    }

    [RelayCommand]
    private async Task OnTouched(IConfirmationViewModel account)
    {
        _needRefresh = true;

        await Shell.Current.GoToAsync($"{nameof(ConfirmationsPage)}");
        _messenger.Send(new UpdateAccountConfirmationPageMessage(account));
    }
}