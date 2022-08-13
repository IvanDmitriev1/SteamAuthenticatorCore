using System.Threading.Tasks;
using System.Windows;
using SteamAuthenticatorCore.Desktop.Services;
using SteamAuthenticatorCore.Shared;
using Wpf.Ui.Mvvm.Contracts;

namespace SteamAuthenticatorCore.Desktop.Views;

public partial class Container
{
    public Container(INavigationService navigationService, IPageService pageService, AppSettings appSettings, ISnackbarService snackbarService, IDialogService dialogService, TaskBarServiceWrapper taskBarServiceWrapper)
    {
        InitializeComponent();

        _appSettings = appSettings;
        _taskBarServiceWrapper = taskBarServiceWrapper;

        navigationService.SetNavigationControl(NavigationFluent);
        navigationService.SetPageService(pageService);

        snackbarService.SetSnackbarControl(RootSnackbar);
        dialogService.SetDialogControl(RootDialog);

        NavigationFluent.Loaded += NavigationFluentOnLoaded;
    }

    private readonly AppSettings _appSettings;
    private readonly TaskBarServiceWrapper _taskBarServiceWrapper;

    private async void NavigationFluentOnLoaded(object sender, RoutedEventArgs e)
    {
        await Task.Run(_appSettings.LoadSettings);

        RootWelcomeGrid.Visibility = Visibility.Hidden;
        MainContent.Visibility = Visibility.Visible;
        _taskBarServiceWrapper.SetActiveWindow(this);
    }
}