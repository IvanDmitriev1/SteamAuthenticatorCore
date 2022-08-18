using System.Windows;
using SteamAuthenticatorCore.Desktop.Services;
using SteamAuthenticatorCore.Shared;
using SteamAuthenticatorCore.Shared.Services;
using Wpf.Ui.Mvvm.Contracts;

namespace SteamAuthenticatorCore.Desktop.Views;

public partial class Container
{
    public Container(INavigationService navigationService, IPageService pageService, AppSettings appSettings, ISnackbarService snackbarService, IDialogService dialogService, TaskBarServiceWrapper taskBarServiceWrapper, ConfirmationServiceBase confirmationServiceBase)
    {
        InitializeComponent();

        _appSettings = appSettings;
        _taskBarServiceWrapper = taskBarServiceWrapper;
        _confirmationServiceBase = confirmationServiceBase;

        navigationService.SetNavigationControl(NavigationFluent);
        navigationService.SetPageService(pageService);

        snackbarService.SetSnackbarControl(RootSnackbar);
        dialogService.SetDialogControl(RootDialog);

        NavigationFluent.Loaded += NavigationFluentOnLoaded;
    }

    private readonly AppSettings _appSettings;
    private readonly TaskBarServiceWrapper _taskBarServiceWrapper;
    private readonly ConfirmationServiceBase _confirmationServiceBase;

    private void NavigationFluentOnLoaded(object sender, RoutedEventArgs e)
    {
        _appSettings.LoadSettings();
        _confirmationServiceBase.Initialize();

        RootWelcomeGrid.Visibility = Visibility.Hidden;
        MainContent.Visibility = Visibility.Visible;
        _taskBarServiceWrapper.SetActiveWindow(this);
    }
}