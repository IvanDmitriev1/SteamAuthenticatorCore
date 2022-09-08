using SettingsViewModel = SteamAuthenticatorCore.Mobile.ViewModels.SettingsViewModel;

namespace SteamAuthenticatorCore.Mobile.Pages;

public partial class SettingsPage : ContentPage
{
	public SettingsPage(SettingsViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}