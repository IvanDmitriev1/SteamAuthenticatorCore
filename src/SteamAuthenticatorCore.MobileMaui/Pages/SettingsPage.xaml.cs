using SteamAuthenticatorCore.MobileMaui.ViewModels;

namespace SteamAuthenticatorCore.MobileMaui.Pages;

public partial class SettingsPage : ContentPage
{
	public SettingsPage(SettingsViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}