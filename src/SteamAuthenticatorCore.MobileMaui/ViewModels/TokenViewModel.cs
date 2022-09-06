using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SteamAuthCore.Abstractions;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.ViewModel;

namespace SteamAuthenticatorCore.MobileMaui.ViewModels;

public partial class TokenViewModel : TokenViewModelBase
{
    public TokenViewModel(ObservableCollection<SteamGuardAccount> accounts, IPlatformTimer platformTimer, IMessenger messenger, AccountsFileServiceResolver accountsFileServiceResolver, ISteamGuardAccountService accountService) : base(accounts, platformTimer)
    {
        _messenger = messenger;
        _accountsFileServiceResolver = accountsFileServiceResolver;
        _accountService = accountService;

        Token = "Login token";
        IsMobile = true;
    }

    private readonly IMessenger _messenger;
    private readonly AccountsFileServiceResolver _accountsFileServiceResolver;
    private readonly ISteamGuardAccountService _accountService;

    [RelayCommand]
    public async Task Import()
    {
        IEnumerable<FileResult> files;

        try
        {
            files = await FilePicker.PickMultipleAsync(new PickOptions()
            {
                PickerTitle = "Select maFile"
            }) ?? Enumerable.Empty<FileResult>();
        }
        catch
        {
            files = Enumerable.Empty<FileResult>();
        }

        var accountsFileService = _accountsFileServiceResolver.Invoke();

        foreach (var fileResult in files)
        {
            await using var stream = await fileResult.OpenReadAsync().ConfigureAwait(false);
            await accountsFileService.SaveAccount(stream, fileResult.FileName).ConfigureAwait(false);
        }
    }

    [RelayCommand]
    public void OnPress(SteamGuardAccount account)
    {
        SelectedAccount = account;
    }

    [RelayCommand]
    public async Task Copy(string token)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrWhiteSpace(token) || token == "Login token")
            return;

        try
        {
            HapticFeedback.Perform(HapticFeedbackType.LongPress);
        }
        catch
        {
            //
        }

        await Clipboard.SetTextAsync(Token);

        var toast = Toast.Make("Login token copied");
        await toast.Show();
    }
}
