using System.Threading.Tasks;
using System.Windows;
using SteamAuthenticatorCore.Shared;
using SteamAuthenticatorCore.Shared.Services;
using Wpf.Ui.Mvvm.Contracts;

namespace SteamAuthenticatorCore.Desktop.Views;

public partial class Container
{
    public Container(INavigationService navigationService, IPageService pageService, AppSettings appSettings)
    {
        InitializeComponent();

        _appSettings = appSettings;
        navigationService.SetNavigationControl(NavigationFluent);
        navigationService.SetPageService(pageService);

        NavigationFluent.Loaded += NavigationFluentOnLoaded;
    }

    private readonly AppSettings _appSettings;

    private async void NavigationFluentOnLoaded(object sender, RoutedEventArgs e)
    {
        await Task.Run(_appSettings.LoadSettings);

        RootWelcomeGrid.Visibility = Visibility.Hidden;
        MainContent.Visibility = Visibility.Visible;
    }
}