using SteamAuthenticatorCore.MobileMaui.ViewModels;

namespace SteamAuthenticatorCore.MobileMaui.Pages;

public partial class SettingsPage : ContentPage
{
	public SettingsPage(SettingsViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }

	private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
	{

    }
}