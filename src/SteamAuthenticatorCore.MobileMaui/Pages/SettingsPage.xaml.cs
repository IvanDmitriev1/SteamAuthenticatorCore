using SteamAuthenticatorCore.MobileMaui.ViewModels;
using SettingsViewModel = SteamMobileAuthenticator.ViewModels.SettingsViewModel;

namespace SteamAuthenticatorCore.MobileMaui.Pages;

public partial class SettingsPage : ContentPage
{
	public SettingsPage(SettingsViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}