using SteamAuthenticatorCore.Shared.Abstractions;

namespace SteamAuthenticatorCore.Desktop.Views.Pages;

public partial class ConfirmationsPage
{
    public ConfirmationsPage(IConfirmationViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
