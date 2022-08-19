using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SteamAuthenticatorCore.Desktop.Extensions;
using SteamAuthenticatorCore.Desktop.Services;
using SteamAuthenticatorCore.Desktop.Views.Pages;
using SteamAuthenticatorCore.Shared;
using SteamAuthenticatorCore.Shared.Abstraction;
using SteamAuthenticatorCore.Shared.Services;
using Wpf.Ui.Mvvm.Contracts;

namespace SteamAuthenticatorCore.Desktop.Views;

public partial class Container
{
    public Container(INavigationService navigationService, IPageService pageService, AppSettings appSettings, ISnackbarService snackbarService, IDialogService dialogService, TaskBarServiceWrapper taskBarServiceWrapper, ConfirmationServiceBase confirmationServiceBase, IUpdateService updateService)
    {
        InitializeComponent();

        _appSettings = appSettings;
        _taskBarServiceWrapper = taskBarServiceWrapper;
        _confirmationServiceBase = confirmationServiceBase;
        _updateService = updateService;

        navigationService.SetNavigationControl(NavigationFluent);
        navigationService.SetPageService(pageService);

        snackbarService.SetSnackbarControl(RootSnackbar);
        dialogService.SetDialogControl(RootDialog);

        NavigationFluent.Loaded += NavigationFluentOnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;
    }

    private readonly AppSettings _appSettings;
    private readonly TaskBarServiceWrapper _taskBarServiceWrapper;
    private readonly ConfirmationServiceBase _confirmationServiceBase;
    private readonly IUpdateService _updateService;

    private async void NavigationFluentOnLoaded(object sender, RoutedEventArgs e)
    {
        _appSettings.LoadSettings();
        _confirmationServiceBase.Initialize();

        RootWelcomeGrid.Visibility = Visibility.Hidden;
        MainContent.Visibility = Visibility.Visible;
        _taskBarServiceWrapper.SetActiveWindow(this);

        await _updateService.CheckForUpdateAndDownloadInstall(true);

        if (!_appSettings.Updated)
            return;

        await Task.Delay(TimeSpan.FromSeconds(3));
        _updateService.DeletePreviousFile("SteamAuthenticatorCore.Desktop");
        _appSettings.Updated = false;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        NavigationFluent.Loaded -= NavigationFluentOnLoaded;
        Unloaded -= OnUnloaded;
        SizeChanged -= OnSizeChanged;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        NavigationFluent.IsExpanded = !(e.NewSize.Width <= 800);
    }

    private void MenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        var menuItem = (MenuItem)sender;

        switch ((string) menuItem.Tag)
        {
            case "token":
                NavigationFluent.NavigateTo(typeof(TokenPage));
                break;
            case "confirms":
                NavigationFluent.NavigateTo(typeof(ConfirmationsPage));
                break;
            case "settings":
                NavigationFluent.NavigateTo(typeof(SettingsPage));
                break;
            case "close":
                Application.Current.Shutdown();
                break;
        }
    }
}