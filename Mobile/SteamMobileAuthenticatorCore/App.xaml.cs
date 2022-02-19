using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SteamAuthCore;
using SteamMobileAuthenticatorCore.Services;
using SteamAuthCore.Manifest;
using SteamMobileAuthenticatorCore.Helpers;
using SteamMobileAuthenticatorCore.Views;
using Xamarin.Forms;

namespace SteamMobileAuthenticatorCore
{
    public partial class App : Application
    {
        public static IManifestModelService ManifestModelService { get; private set; } = null!;
        public static ObservableCollection<SteamGuardAccount> Accounts { get; private set; } = null!;
        public static Timer AutoMarketSellTimer { get; private set; } = null!;

        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
            Accounts = new ObservableCollection<SteamGuardAccount>();
            AutoMarketSellTimer = new Timer(AutoTradeConfirmationTimerOnTick);
            ManifestModelService = new LocalDriveManifestModelService(new MobileDirectoryService());
        }

        protected override void OnStart()
        {

        }

        protected override void OnSleep()
        {
            ManifestModelService.SaveManifest();
        }

        protected override void OnResume()
        {

        }

        private static async Task AutoTradeConfirmationTimerOnTick()
        {
            Dictionary<SteamGuardAccount, List<ConfirmationModel>> autoAcceptConfirmations = new();
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
            }
        }
    }
}
