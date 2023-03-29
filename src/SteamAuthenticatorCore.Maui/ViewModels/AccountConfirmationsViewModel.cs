using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Shared.ViewModel;
using System.Collections.ObjectModel;

namespace SteamAuthenticatorCore.Mobile.ViewModels;

public sealed partial class AccountConfirmationsViewModel : BaseAccountConfirmationsViewModel, IDisposable
{
    public AccountConfirmationsViewModel(ISteamGuardAccountService accountService) : base(accountService)
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

        if (Model?.Confirmations.Count == 0)
            await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private Task OnElementTouch(VisualElement view)
    {
        var item = (view, (ConfirmationModel) view.BindingContext);

        if (SelectedItems.Contains(item))
        {
            SelectedItems.Remove(item);

            if (SelectedItems.Count == 0)
                IsCountTitleViewVisible = false;

            //await view.ChangeBackgroundColorToWithColorsCollection("SecondBackgroundColor");
            return Task.CompletedTask;
        }

        SelectedItems.Add(item);
        IsCountTitleViewVisible = true;

        //await view.ChangeBackgroundColorToWithColorsCollection("SecondBackgroundSelectionColor");
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ConfirmSelected() => await SendConfirmationsInternal(ConfirmationOptions.Allow);

    [RelayCommand]
    private async Task CancelSelected() => await SendConfirmationsInternal(ConfirmationOptions.Deny);

    private async ValueTask SendConfirmationsInternal(ConfirmationOptions confirmationOptions)
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
            await SendConfirmations(new[] { SelectedItems[0].Item2 }, confirmationOptions);
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