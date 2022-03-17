using System.Windows.Controls;
using SteamAuthenticatorCore.Desktop.ViewModels;

namespace SteamAuthenticatorCore.Desktop.Views.Pages;

public partial class ConfirmationsPage : Page
{
    public ConfirmationsPage(ConfirmationViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}