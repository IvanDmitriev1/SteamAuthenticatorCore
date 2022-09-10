using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Mobile.Extensions;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.ViewModel;

namespace SteamAuthenticatorCore.Mobile.ViewModels;

public sealed partial class ConfirmationViewModel : ConfirmationsViewModelBase, IDisposable
{
    public ConfirmationViewModel(ISteamGuardAccountService accountService, IPlatformImplementations platformImplementations, IMessenger messenger) : base(accountService, platformImplementations, messenger)
    {
        SelectedItems = new ObservableCollection<(VisualElement, ConfirmationModel)>();
    }

    [ObservableProperty]
    private bool _isCountTitleViewVisible;

    public ObservableCollection<(VisualElement, ConfirmationModel)> SelectedItems { get; }

    public void Dispose()
    {
        SelectedItems.Clear();
    }

    [RelayCommand]
    private async Task HideCountTitleView()
    {
        SelectedItems.Clear();
        IsCountTitleViewVisible = false;

        if (ConfirmationModel.Confirmations.Count == 0)
            await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task OnElementTouch(VisualElement view)
    {
        var item = (view, (ConfirmationModel) view.BindingContext);

        if (SelectedItems.Contains(item))
        {
            SelectedItems.Remove(item);

            if (SelectedItems.Count == 0)
                IsCountTitleViewVisible = false;

            await view.ChangeBackgroundColorToWithColorsCollection("SecondBackgroundColor");
            return;
        }

        SelectedItems.Add(item);
        IsCountTitleViewVisible = true;

        await view.ChangeBackgroundColorToWithColorsCollection("SecondBackgroundSelectionColor");
    }

    [RelayCommand]
    private async Task ConfirmSelected() => await SendConfirmations(ConfirmationOptions.Allow);

    [RelayCommand]
    private async Task CancelSelected() => await SendConfirmations(ConfirmationOptions.Deny);

    private async ValueTask SendConfirmations(ConfirmationOptions confirmationOptions)
    {
        if (SelectedItems.Count == 0)
            return;

        try
        {
            HapticFeedback.Perform(HapticFeedbackType.Click);
        }
        catch
        {
            //
        }

        if (SelectedItems.Count == 1)
        {
            await SendConfirmation(SelectedItems[0].Item2, confirmationOptions);
        }
        else
        {
            var items = new ConfirmationModel[SelectedItems.Count];
            for (var i = 0; i < SelectedItems.Count; i++)
                items[i] = SelectedItems[i].Item2;

            await SendConfirmations(items, confirmationOptions);   
        }

        await HideCountTitleView();
    }
}