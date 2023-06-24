namespace SteamAuthenticatorCore.Maui.ViewModels;

public sealed partial class AccountConfirmationsViewModel : BaseAccountConfirmationsViewModel, IDisposable
{
    public AccountConfirmationsViewModel(ISteamGuardAccountService accountService, IPlatformImplementations platformImplementations) : base(accountService, platformImplementations)
    {
        SelectedItems = new ObservableCollection<(VisualElement, Confirmation)>();
    }

    [ObservableProperty]
    private bool _isCountTitleViewVisible;

    public ObservableCollection<(VisualElement, Confirmation)> SelectedItems { get; }

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
        var item = (view, (Confirmation) view.BindingContext);

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

        var items = new Confirmation[SelectedItems.Count];
        for (var i = 0; i < SelectedItems.Count; i++)
            items[i] = SelectedItems[i].Item2;

        await SendConfirmations(items, confirmationOptions);

        await HideCountTitleView();
    }
}