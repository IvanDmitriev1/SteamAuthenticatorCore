using SteamAuthenticatorCore.Desktop.ViewModels;

namespace SteamAuthenticatorCore.Desktop.Views.Pages;

public partial class SettingsPage
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
    }
}
