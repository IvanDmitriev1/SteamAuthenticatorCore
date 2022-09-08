using SteamMobileAuthenticator.ViewModels;

namespace SteamAuthenticatorCore.MobileMaui.Pages;

public partial class ConfirmationsPage : ContentPage
{
	public ConfirmationsPage(ConfirmationViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}