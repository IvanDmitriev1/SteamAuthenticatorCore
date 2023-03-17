using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SteamAuthenticatorCore.Desktop.Helpers;
using SteamAuthenticatorCore.Desktop.Views.Pages;
using SteamAuthenticatorCore.Shared;
using SteamAuthenticatorCore.Shared.Abstractions;
using Wpf.Ui.Appearance;

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
        NavigationFluent.SetServiceProvider(App.ServiceProvider);

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

        //RootWelcomeGrid.Visibility = Visibility.Hidden;
        NavigationFluent.Visibility = Visibility.Visible;
        NavigationFluent.Navigate(typeof(TokenPage));

        //await _updateService.CheckForUpdateAndDownloadInstall(true);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        NavigationFluent.Loaded -= NavigationFluentOnLoaded;
        Unloaded -= OnUnloaded;
        SizeChanged -= OnSizeChanged;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        NavigationFluent.IsPaneOpen = !(e.NewSize.Width <= 800);
    }

    private void MenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        //TODo
        /*var menuItem = (MenuItem)sender;

        switch ((string) menuItem.Tag)
        {
            case "token":
                NavigationFluent.NavigateTo(typeof(TokenPage));
                break;
            case "confirms":
                NavigationFluent.NavigateTo(typeof(ConfirmationsOverviewPage));
                break;
            case "settings":
                NavigationFluent.NavigateTo(typeof(SettingsPage));
                break;
            case "close":
                Application.Current.Shutdown();
                break;
        }*/
    }
}