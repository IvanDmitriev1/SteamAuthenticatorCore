using SteamAuthenticatorCore.Mobile.ViewModels;

namespace SteamAuthenticatorCore.Mobile.Pages;

public partial class SettingsPage : ContentPage
{
	public SettingsPage(SettingsViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}