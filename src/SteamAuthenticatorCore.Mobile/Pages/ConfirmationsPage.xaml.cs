using Microsoft.Extensions.DependencyInjection;
using SteamAuthenticatorCore.Mobile.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SteamAuthenticatorCore.Mobile.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConfirmationsPage : ContentPage
    {
        public ConfirmationsPage()
        {
            InitializeComponent();
            BindingContext = Startup.ServiceProvider.GetRequiredService<ConfirmationViewModel>();
        }
    }
}