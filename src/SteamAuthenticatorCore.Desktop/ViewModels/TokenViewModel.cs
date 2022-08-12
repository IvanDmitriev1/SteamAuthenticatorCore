using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using SteamAuthCore;
using SteamAuthenticatorCore.Shared.Abstraction;
using SteamAuthenticatorCore.Shared.Services;
using SteamAuthenticatorCore.Shared.ViewModel;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public partial class TokenViewModel : TokenViewModelBase
{
    public TokenViewModel(ObservableCollection<SteamGuardAccount> accounts, IPlatformTimer platformTimer, ManifestAccountsWatcherService accountsWatcherService) : base(accounts, platformTimer)
    {
        _accountsWatcherService = accountsWatcherService;
        OnLoadedCommand = new AsyncRelayCommand<StackPanel>(OnPageLoaded);
    }

    private readonly ManifestAccountsWatcherService _accountsWatcherService;

    public ICommand OnLoadedCommand { get; }

    private async Task OnPageLoaded(StackPanel? stackPanel)
    {
        if (Accounts.Count > 0)
        {
            stackPanel!.Visibility = Visibility.Hidden;
            return;
        }

        await _accountsWatcherService.Initialize();
        stackPanel!.Visibility = Visibility.Hidden;
    }
}