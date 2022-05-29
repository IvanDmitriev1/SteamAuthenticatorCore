using System;
using System.Threading.Tasks;
using SteamAuthenticatorCore.Mobile.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SteamAuthenticatorCore.Mobile.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SettingsPage : ContentPage, IBackButtonAction
	{
		public SettingsPage ()
        {
            InitializeComponent();
            OnBackActionAsync = OnBackActionAsyncFunc;
        }

        public Func<Task<bool>>? OnBackActionAsync { get; set; }

        private async Task<bool> OnBackActionAsyncFunc()
        {
            await Shell.Current.GoToAsync($"//{nameof(TokenPage)}");
            return true;
        }
    }
}