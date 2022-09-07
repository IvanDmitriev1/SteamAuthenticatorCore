using SteamAuthenticatorCore.MobileMaui.ViewModels;

namespace SteamAuthenticatorCore.MobileMaui.Pages;

public partial class ConfirmationsOverviewPage : ContentPage
{
	public ConfirmationsOverviewPage(ConfirmationsOverviewViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}