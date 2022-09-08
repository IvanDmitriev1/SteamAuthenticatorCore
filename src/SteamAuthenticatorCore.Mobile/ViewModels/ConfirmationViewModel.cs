using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.Messages;

namespace SteamAuthenticatorCore.Mobile.ViewModels;

public partial class ConfirmationViewModel : ObservableObject, IRecipient<UpdateAccountConfirmationPageMessage>, IDisposable
{
    public ConfirmationViewModel(IMessenger messenger, ISteamGuardAccountService accountService)
    {
        _accountService = accountService;
        messenger.Register(this);
        SelectedItems = new ObservableCollection<(View, ConfirmationModel)>();
    }

    private readonly ISteamGuardAccountService _accountService;

    [ObservableProperty]
    private IConfirmationViewModel _account = null!;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private bool _isCountTitleViewVisible;

    public ObservableCollection<(View, ConfirmationModel)> SelectedItems { get; }

    public void Receive(UpdateAccountConfirmationPageMessage message)
    {
        Account = message.Value;
    }

    public void Dispose()
    {
        SelectedItems.Clear();
    }

    [RelayCommand]
    private async Task HideCountTitleView()
    {
        var tasks = new Task[SelectedItems.Count];

        var value = Application.Current!.RequestedTheme == AppTheme.Light
            ? "SecondLightBackgroundColor"
            : "SecondDarkBackground";

        Application.Current!.Resources.TryGetValue(value, out var color);

        for (var i = 0; i < SelectedItems.Count; i++)
        {
            var view = SelectedItems[i].Item1;
            tasks[i] = view.BackgroundColorTo((Color)color!, 16, 150);
        }

        SelectedItems.Clear();
        IsCountTitleViewVisible = false;
        await Task.WhenAll(tasks);

        if (_account.Confirmations.Count == 0)
            await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private Task OnElementTouch(View view)
    {
        var item = (view, (ConfirmationModel) view.BindingContext);

        if (SelectedItems.Contains(item))
        {
            SelectedItems.Remove(item);

            if (SelectedItems.Count == 0)
                IsCountTitleViewVisible = false;

            var value = Application.Current!.RequestedTheme == AppTheme.Light
                ? "SecondLightBackgroundColor"
                : "SecondDarkBackground";

            Application.Current!.Resources.TryGetValue(value, out var color);

            return view.BackgroundColorTo((Color)color!, 16, 150);
        }

        SelectedItems.Add(item);
        IsCountTitleViewVisible = true;

        var value2 = Application.Current!.RequestedTheme == AppTheme.Light
            ? "SecondLightBackgroundSelectionColor"
            : "SecondDarkBackgroundSelectionColor";

        Application.Current!.Resources.TryGetValue(value2, out var color2);

        return view.BackgroundColorTo((Color)color2!, 16, 150);
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
    private async Task ConfirmSelected()
    {
        switch (SelectedItems.Count)
        {
            case 0:
                return;
            case 1:
                await SendConfirmation(ConfirmationOptions.Allow);
                break;
            default:
                await SendConfirmations(ConfirmationOptions.Allow);
                break;
        }

        await HideCountTitleView();
    }

    [RelayCommand]
    private async Task CancelSelected()
    {
        switch (SelectedItems.Count)
        {
            case 0:
                return;
            case 1:
                await SendConfirmation(ConfirmationOptions.Deny);
                break;
            default:
                await SendConfirmations(ConfirmationOptions.Deny);
                break;
        }

        await HideCountTitleView();
    }

    private async Task SendConfirmation(ConfirmationOptions confirmation)
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

        if (await _accountService.SendConfirmation(Account.Account, model.Item2, confirmation, CancellationToken.None))
        {
            Account.Confirmations.Remove(model.Item2);
            SelectedItems.Clear();
        }
    }

    private async Task SendConfirmations(ConfirmationOptions confirmation)
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

        if (await _accountService.SendConfirmation(Account.Account, items, confirmation, CancellationToken.None))
        {
            foreach (var item in items)
                Account.Confirmations.Remove(item);

            SelectedItems.Clear();
        }
    }
}