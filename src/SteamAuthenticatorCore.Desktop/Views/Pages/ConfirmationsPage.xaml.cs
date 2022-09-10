using SteamAuthenticatorCore.Desktop.ViewModels;

namespace SteamAuthenticatorCore.Desktop.Views.Pages;

public partial class ConfirmationsPage
{
    public ConfirmationsPage(ConfirmationsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
