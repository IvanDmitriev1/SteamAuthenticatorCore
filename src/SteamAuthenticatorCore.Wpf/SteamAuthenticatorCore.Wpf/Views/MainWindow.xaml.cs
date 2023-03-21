using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SteamAuthCore.Abstractions;
using SteamAuthenticatorCore.Desktop.Helpers;
using SteamAuthenticatorCore.Desktop.Services;
using SteamAuthenticatorCore.Desktop.Views.Pages;
using SteamAuthenticatorCore.Shared;
using SteamAuthenticatorCore.Shared.Abstractions;
using Wpf.Ui.Appearance;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Wpf.Ui.Controls.IconElements;

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
        AppSettings.Current.Load();

        await App.ServiceProvider.GetRequiredService<ITimeAligner>().AlignTimeAsync();
        await App.ServiceProvider.GetRequiredService<AccountsServiceResolver>().Invoke().Initialize();
        await App.ServiceProvider.GetRequiredService<IConfirmationService>().Initialize();
    }
}