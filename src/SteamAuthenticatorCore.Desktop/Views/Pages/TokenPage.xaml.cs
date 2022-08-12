using System.Windows.Controls;
using SteamAuthenticatorCore.Desktop.ViewModels;

namespace SteamAuthenticatorCore.Desktop.Views.Pages;

public partial class TokenPage : Page
{
    public TokenPage(TokenViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}