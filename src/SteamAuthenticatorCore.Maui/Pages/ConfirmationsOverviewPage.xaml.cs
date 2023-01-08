using SteamAuthenticatorCore.Mobile.ViewModels;

namespace SteamAuthenticatorCore.Mobile.Pages;

public partial class ConfirmationsOverviewPage : ContentPage
{
	public ConfirmationsOverviewPage(ConfirmationsOverviewViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}