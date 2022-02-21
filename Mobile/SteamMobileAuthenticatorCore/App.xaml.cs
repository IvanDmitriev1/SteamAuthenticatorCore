using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SteamAuthCore;
using SteamAuthCore.Manifest;
using Xamarin.Forms;

namespace SteamAuthenticatorCore.Mobile
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();

            DependencyService.Register<IManifestModelService, LocalDriveManifestModelService>();
            DependencyService.Register<ObservableCollection<SteamGuardAccount>>();
        }

        protected override void OnStart()
        {
            
        }

        protected override void OnSleep()
        {
            DependencyService.Get<IManifestModelService>().SaveManifest();
        }

        protected override void OnResume()
        {

        }

        private static async Task AutoTradeConfirmationTimerOnTick()
        {
            /*Dictionary<SteamGuardAccount, List<ConfirmationModel>> autoAcceptConfirmations = new();
            SteamGuardAccount[] accounts = Accounts.ToArray();

            foreach (var account in accounts)
            {
                try
                {
                    foreach (var confirmationModel in await account.FetchConfirmationsAsync())
                    {
                        if (confirmationModel.ConfType == ConfirmationModel.ConfirmationType.MarketSellTransaction || ManifestModelService.GetManifestModel().AutoConfirmMarketTransactions)
                        {
                            if (!autoAcceptConfirmations.ContainsKey(account))
                                autoAcceptConfirmations[account] = new List<ConfirmationModel>();

                            autoAcceptConfirmations[account].Add(confirmationModel);
                        }
                    }
                }
                catch (SteamGuardAccount.WgTokenInvalidException)
                {
                    await account.RefreshSessionAsync();
                }
                catch (SteamGuardAccount.WgTokenExpiredException)
                {
                    //Prompt to relogin
                    if (await Application.Current.MainPage.DisplayAlert("Auto-market confirmation", $"You must re-log in to {account.AccountName} account", "Re-log", "not now"))
                        await Shell.Current.GoToAsync($"{nameof(LoginPage)}?id={Accounts.IndexOf(account)}");
                    break;

                }
                catch
                {
                    //
                }
            }

            foreach (var account in autoAcceptConfirmations.Keys)
            {
                account.SendConfirmationAjax(autoAcceptConfirmations[account], SteamGuardAccount.Confirmation.Allow);
            }*/
        }
    }
}
