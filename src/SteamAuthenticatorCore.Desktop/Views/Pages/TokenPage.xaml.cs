using SteamAuthenticatorCore.Desktop.ViewModels;

namespace SteamAuthenticatorCore.Desktop.Views.Pages;

public partial class TokenPage
{
    public TokenPage(TokenViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
