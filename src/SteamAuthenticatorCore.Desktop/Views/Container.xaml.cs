using Wpf.Ui.Mvvm.Contracts;

namespace SteamAuthenticatorCore.Desktop.Views;

public partial class Container
{
    public Container(INavigationService navigationService, IPageService pageService)
    {
        InitializeComponent();

        navigationService.SetNavigationControl(NavigationFluent);
        navigationService.SetPageService(pageService);
    }
}
