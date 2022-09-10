using SteamAuthenticatorCore.Desktop.ViewModels;

namespace SteamAuthenticatorCore.Desktop.Views.Pages;

public partial class ConfirmationsOverviewPage
{
    public ConfirmationsOverviewPage(ConfirmationsOverviewViewModel overviewViewModel)
    {
        DataContext = overviewViewModel;
        InitializeComponent();
    }
}
