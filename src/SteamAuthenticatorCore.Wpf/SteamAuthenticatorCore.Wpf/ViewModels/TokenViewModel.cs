using System.IO;
using System.Threading;
using Microsoft.Win32;
using Wpf.Ui.TaskBar;

namespace SteamAuthenticatorCore.Desktop.ViewModels;

public sealed partial class TokenViewModel : MyObservableRecipient, IAsyncDisposable
{
    public TokenViewModel(ISteamGuardAccountService steamAccountService,
        AccountsServiceResolver accountsServiceResolver, IBackgroundTimerFactory backgroundTimerFactory)
    {
        _steamAccountService = steamAccountService;
        _accountsService = accountsServiceResolver.Invoke();
        _appSettings = AppSettings.Current;

        _backgroundTimer = backgroundTimerFactory.StartNewTimer(TimeSpan.FromSeconds(1), OnTimer);
    }

    private readonly ISteamGuardAccountService _steamAccountService;
    private readonly IBackgroundTimer _backgroundTimer;
    private readonly IAccountsService _accountsService;
    private readonly AppSettings _appSettings;
    private StackPanel? _stackPanel;
    private Int64 _currentSteamChunk;
    private IReadOnlyList<SteamGuardAccount> _accounts = Array.Empty<SteamGuardAccount>();
    private string _searchBoxText = string.Empty;

    public string SearchBoxText
    {
        get => _searchBoxText;
        set
        {
            SetProperty(ref _searchBoxText, value);
            UpdateSearchResults(value);
        }
    }

    public ObservableCollection<SteamGuardAccount> FilteredAccounts { get; } = new();

    [ObservableProperty]
    private string _token = string.Empty;

    [ObservableProperty]
    private double _tokenProgressBar;

    [ObservableProperty]
    private SteamGuardAccount? _selectedAccount;

    protected override void OnDeactivated()
    {
        base.OnDeactivated();

        _stackPanel = null;
    }

    public async ValueTask DisposeAsync()
    {
        await _backgroundTimer.DisposeAsync().ConfigureAwait(false);
    }

    #region RelayCommands

    [RelayCommand]
    private async Task PageLoaded(StackPanel stackPanel)
    {
        _stackPanel = stackPanel;

        await RefreshAccounts();
    }

    [RelayCommand]
    private async Task RefreshAccounts()
    {
        Token = string.Empty;
        TokenProgressBar = 0;

        _stackPanel!.Visibility = Visibility.Visible;
        TaskBarService.Default.SetState(TaskBarProgressState.Indeterminate);

        try
        {
            await _accountsService.Initialize();
            _accounts = await _accountsService.GetAll();
            UpdateSearchResults(string.Empty);
        }
        finally
        {
            TaskBarService.Default.SetState(TaskBarProgressState.None);
            _stackPanel!.Visibility = Visibility.Hidden;
        }
    }

    [RelayCommand]
    private async Task ImportAccounts()
    {
        OpenFileDialog fileDialog = new()
        {
            Multiselect = true,
            CheckFileExists = true,
            Filter = $"mafile| *{IAccountsService.AccountFileExtension}"
        };

        if (fileDialog.ShowDialog() == false)
            return;

        await SaveAccountsFromFilesNames(fileDialog.FileNames);
    }

    [RelayCommand]
    private async Task ForceRefreshSession()
    {
        if (SelectedAccount is null)
            return;

        var dialog = ContentDialogService.Default.CreateDialog();
        dialog.Title = _appSettings.LocalizationProvider[LocalizationMessage.RefreshSessionMessage];

        if (!await _steamAccountService.RefreshSession(SelectedAccount, CancellationToken.None))
        {
            dialog.Content = _appSettings.LocalizationProvider[LocalizationMessage.FailedToRefreshSessionMessage];
        }
        else
        {
            await _accountsService.Update(SelectedAccount);
            dialog.Content = _appSettings.LocalizationProvider[LocalizationMessage.SessionHasBeenRefreshedMessage];   
        }

        await dialog.ShowAsync();
    }

    [RelayCommand]
    private async void ShowAccountFilesFolder()
    {
        if (_appSettings.AccountsLocation == AccountsLocation.GoogleDrive)
        {
            var dialog = ContentDialogService.Default.CreateDialog();
            dialog.Title = _appSettings.LocalizationProvider[LocalizationMessage.ErrorMessage];
            dialog.Content =
                string.Format(
                    _appSettings.LocalizationProvider[LocalizationMessage.ShowGoogleAccountFilesFolderContentMessage],
                    App.InternalName);

            await dialog.ShowAsync();
            return;
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                Arguments = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SteamAuthenticatorCore.Desktop"),
                FileName = "explorer.exe"
            };

            Process.Start(startInfo);
        }
        catch (Exception)
        {
            //
        }
    }

    [RelayCommand]
    private async Task DeleteAccount()
    {
        if (SelectedAccount is null)
            return;

        var dialog = ContentDialogService.Default.CreateDialog();
        dialog.Title = _appSettings.LocalizationProvider[LocalizationMessage.DeletingAccountMessage];
        dialog.Content = string.Format(
            _appSettings.LocalizationProvider[LocalizationMessage.DeletingAccountContentMessage],
            SelectedAccount.AccountName);
        dialog.PrimaryButtonText = _appSettings.LocalizationProvider[LocalizationMessage.YesMessage];
        dialog.CloseButtonText = _appSettings.LocalizationProvider[LocalizationMessage.NoMessage];

        if (await dialog.ShowAsync() != ContentDialogResult.Primary)
            return;

        await _accountsService.Delete(SelectedAccount);
        SelectedAccount = null;

        await RefreshAccounts();
    }

    [RelayCommand]
    private void LoginAgain(SteamGuardAccount? account)
    {
        if (account is null)
            return;

        NavigationService.Default.NavigateWithHierarchy(typeof(LoginPage));
        Messenger.Send(new UpdateAccountInLoginPageMessage(account));
    }

    [RelayCommand]
    private static void ListBoxDragOver(DragEventArgs eventArgs)
    {
        if (!eventArgs.Data.GetDataPresent(DataFormats.FileDrop))
            return;

        if (eventArgs.Data.GetData(DataFormats.FileDrop) is not string[] files)
            return;

        foreach (var file in files)
        {
            var extension = Path.GetExtension(file);

            if (Directory.Exists(file))
            {
                eventArgs.Effects = DragDropEffects.None;
                eventArgs.Handled = true;
                return;
            }

            if (extension.Contains(IAccountsService.AccountFileExtension))
                continue;

            eventArgs.Effects = DragDropEffects.None;
            eventArgs.Handled = true;
            return;
        }

        eventArgs.Effects = DragDropEffects.Copy;
        eventArgs.Handled = true;
    }

    [RelayCommand]
    private async Task ListBoxDragAndDrop(DragEventArgs eventArgs)
    {
        if (!eventArgs.Data.GetDataPresent(DataFormats.FileDrop))
            return;

        if (eventArgs.Data.GetData(DataFormats.FileDrop) is not string[] files)
            return;

        await SaveAccountsFromFilesNames(files);
    }

    #endregion

    private void UpdateSearchResults(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            FilteredAccounts.Clear();

            foreach (var account in _accounts)
                FilteredAccounts.Add(account);

            return;
        }

        var suitableItems = new List<SteamGuardAccount>();
        var splitText = text.ToLower().Split(' ');

        foreach (var account in _accounts)
        {
            var itemText = account.AccountName;

            var found = splitText.All(key=> itemText.ToLower().Contains(key));

            if (found)
                suitableItems.Add(account);
        }

        FilteredAccounts.Clear();

        foreach (var account in suitableItems)
            FilteredAccounts.Add(account);
    }

    private async ValueTask SaveAccountsFromFilesNames(string[] files)
    {
        foreach (var fileName in files)
        {
            using var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            await _accountsService.Save(stream, Path.GetFileName(fileName));
        }

        await RefreshAccounts();
    }

    private void OnTimer(CancellationToken obj)
    {
        if (SelectedAccount is null)
            return;

        var steamTime = ITimeAligner.SteamTime;
        _currentSteamChunk = steamTime / 30L;
        var secondsUntilChange = (int)(steamTime - _currentSteamChunk * 30L);

        if (SelectedAccount.GenerateSteamGuardCode() is { } token)
            Token = token;

        TokenProgressBar = 30 - secondsUntilChange;
    }
}