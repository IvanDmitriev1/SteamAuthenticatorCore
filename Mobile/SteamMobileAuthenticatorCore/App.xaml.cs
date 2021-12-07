﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SteamAuthCore;
using SteamMobileAuthenticatorCore.Services;
using SteamAuthCore.Manifest;
using SteamMobileAuthenticatorCore.Helpers;
using Xamarin.Forms;

namespace SteamMobileAuthenticatorCore
{
    public partial class App : Application
    {
        public static IManifestModelService ManifestModelService { get; private set; } = null!;
        public static ObservableCollection<SteamGuardAccount> Accounts { get; private set; } = null!;
        public static Timer AutoMarketSetTimer { get; private set; } = null!;

        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
            Accounts = new ObservableCollection<SteamGuardAccount>();
            AutoMarketSetTimer = new Timer(AutoTradeConfirmationTimerOnTick);
            ManifestModelService = new LocalDriveManifestModelService(new MobileDirectoryService());
        }

        protected override void OnStart()
        {
            
        }

        protected override void OnSleep()
        {
            
        }

        protected override void OnResume()
        {

        }

        private static async Task AutoTradeConfirmationTimerOnTick()
        {
            Dictionary<SteamGuardAccount, List<ConfirmationModel>> autoAcceptConfirmations = new();

            foreach (var account in Accounts)
            {
                try
                {
                    ConfirmationModel[] tmp = await account.FetchConfirmationsAsync();
                    foreach (var confirmationModel in tmp)
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
