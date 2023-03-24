using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Shared.Abstractions;

namespace SteamAuthenticatorCore.Mobile.ViewModels;

public partial class TokenViewModel : ObservableRecipient
{
    public TokenViewModel(AccountsServiceResolver accountsFileServiceResolver)
    {
        _accountsService = accountsFileServiceResolver.Invoke();
        _token = "Token";
    }

    private readonly IAccountsService _accountsService;

    private VisualElement? _longPressView;
    private bool _pressed;

    [ObservableProperty]
    private IReadOnlyList<SteamGuardAccount> _accounts = Array.Empty<SteamGuardAccount>();

    [ObservableProperty]
    private string _token;

    [ObservableProperty]
    private bool _isLongPressTitleViewVisible;

    protected override async void OnActivated()
    {
        base.OnActivated();

        Accounts = await _accountsService.GetAll();
    }

    [RelayCommand]
    private async Task Import()
    {
        IEnumerable<FileResult> files;

        try
        {
            files = await FilePicker.PickMultipleAsync(new PickOptions()
            {
                PickerTitle = "Select maFile"
            }).ConfigureAwait(false);
        }
        catch
        {
            files = Enumerable.Empty<FileResult>();
        }

        foreach (var fileResult in files)
        {
            await using var stream = await fileResult.OpenReadAsync().ConfigureAwait(false);
            await _accountsService.Save(stream, fileResult.FileName).ConfigureAwait(false);
        }

        Accounts = await _accountsService.GetAll();
    }
}
