using SteamAuthCore.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SteamAuthenticatorAndroid.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public SteamGuardAccount? Account { get; set; }

        public LoginPage()
        {
            InitializeComponent();
        }
    }
}