using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SteamAuthenticatorCore.Mobile.Helpers;
using SteamAuthenticatorCore.Mobile.ViewModels;
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
            BindingContext = Startup.ServiceProvider.GetRequiredService<SettingsViewModel>();
        }

        public Func<Task<bool>>? OnBackActionAsync { get; set; }

        private static async Task<bool> OnBackActionAsyncFunc()
        {
            await Shell.Current.GoToAsync($"//{nameof(TokenPage)}");
            return true;
        }
    }
}