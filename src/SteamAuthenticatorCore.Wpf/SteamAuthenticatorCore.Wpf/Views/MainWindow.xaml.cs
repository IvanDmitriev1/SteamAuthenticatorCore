using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
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
    public MainWindow(AppSettings appSettings, IUpdateService updateService)
    {
        InitializeComponent();

        Watcher.Watch(this);

        _appSettings = appSettings;
        _updateService = updateService;

        SnackbarService.Default.SetSnackbarControl(RootSnackbar);
        ContentDialogService.Default.SetContentPresenter(RootContentDialogPresenter);

        NavigationFluent.SetServiceProvider(App.ServiceProvider);
        NavigationService.Default.SetNavigationControl(NavigationFluent);

        NavigationFluent.Loaded += NavigationFluentOnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;
    }

    private readonly AppSettings _appSettings;
    private readonly IUpdateService _updateService;

    private async void NavigationFluentOnLoaded(object sender, RoutedEventArgs e)
    {
        _appSettings.Load();
        App.ServiceProvider.GetRequiredService<IConfirmationService>().Initialize();

        RootWelcomeGrid.Visibility = Visibility.Hidden;
        NavigationFluent.Visibility = Visibility.Visible;
        NavigationFluent.Navigate(typeof(TokenPage));

        if (await _updateService.CheckForUpdate() is not { } release)
            return;

        await RootSnackbar.ShowAsync("Updater", $"A new version available: {release.Name}", new SymbolIcon(SymbolRegular.PhoneUpdate20));
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        NavigationFluent.Loaded -= NavigationFluentOnLoaded;
        Unloaded -= OnUnloaded;
        SizeChanged -= OnSizeChanged;
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
}