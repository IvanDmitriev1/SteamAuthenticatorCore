using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Appearance;
using MenuItem = Wpf.Ui.Controls.MenuItem;

namespace SteamAuthenticatorCore.Desktop.Views;

public partial class MainWindow
{
    public MainWindow(IUpdateService updateService, ILogger<MainWindow> logger)
    {
        InitializeComponent();

        Watcher.Watch(this);

        _updateService = updateService;
        _logger = logger;

        SnackbarService.Default.SetSnackbarControl(RootSnackbar);
        ContentDialogService.Default.SetContentPresenter(RootContentDialogPresenter);

        NavigationFluent.SetServiceProvider(App.ServiceProvider);
        NavigationService.Default.SetNavigationControl(NavigationFluent);

        NavigationFluent.Loaded += NavigationFluentOnLoaded;
        SizeChanged += OnSizeChanged;
    }

    private readonly IUpdateService _updateService;
    private readonly ILogger<MainWindow> _logger;

    private async void NavigationFluentOnLoaded(object sender, RoutedEventArgs e)
    {
        await InitializeDependencies();

        RootWelcomeGrid.Visibility = Visibility.Hidden;
        NavigationFluent.Visibility = Visibility.Visible;
        NavigationFluent.Navigate(typeof(TokenPage));

        try
        {
            if (await _updateService.CheckForUpdate() is not { } release)
                return;

            await RootSnackbar.ShowAsync("Updater", $"A new version available: {release.Name}", new SymbolIcon(SymbolRegular.PhoneUpdate20));
        }
        catch (Exception exception)
        {
            _logger.LogCritical(exception, "Exception after checking for updates");
        }
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        NavigationFluent.IsPaneOpen = !(e.NewSize.Width <= 1100);
    }

    private void MenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        var menuItem = (MenuItem)sender;

        switch ((string) menuItem.Tag)
        {
            case "token":
                NavigationService.Default.NavigateWithHierarchy(typeof(TokenPage));
                break;
            case "confirms":
                NavigationService.Default.NavigateWithHierarchy(typeof(ConfirmationsOverviewPage));
                break;
            case "settings":
                NavigationService.Default.NavigateWithHierarchy(typeof(SettingsPage));
                break;
            case "close":
                Application.Current.Shutdown();
                break;
        }
    }

    private static async Task InitializeDependencies()
    {
        WpfAppSettings.Current.Load();

        await App.ServiceProvider.GetRequiredService<ITimeAligner>().AlignTimeAsync();
        await App.ServiceProvider.GetRequiredService<AccountsServiceResolver>().Invoke().Initialize();
        await App.ServiceProvider.GetRequiredService<IConfirmationService>().Initialize();
    }
}