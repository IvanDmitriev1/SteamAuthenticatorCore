using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows.Input;
using System.Windows.Threading;
using SteamAuthCore.Models;
using SteamDesktopAuthenticatorCore.Custom;
using SteamDesktopAuthenticatorCore.Services;
using SteamDesktopAuthenticatorCore.Views;
using WpfHelper;
using WpfHelper.Commands;

namespace SteamDesktopAuthenticatorCore.ViewModels
{
    class ViewConfirmationsWindowViewModel : BaseViewModel
    {
        public ViewConfirmationsWindowViewModel()
        {
            _manifest = ManifestModelService.GetManifest().Result;

            Confirmations = new ObservableCollection<ConfirmationViewModel>();
        }

        #region Variabels

        private readonly ManifestModel _manifest;

        private SteamGuardAccount? _selectedAccount;

        #endregion

        #region Fields

        public bool WindowIsVisible { get; set; }

        public ObservableCollection<ConfirmationViewModel> Confirmations { get; }

        public SteamGuardAccount? SelectedAccount
        {
            get => _selectedAccount;
            set
            {
                if (WindowIsVisible)
                {
                    Confirmations.Clear();
                    UpdateCommand.Execute(value);
                }

                Set(ref _selectedAccount, value);
            }
        }

        #endregion

        #region Commands
        public ICommand UpdateCommand => new AsyncRelayCommand(async o =>
        {
            if (_manifest.AutoConfirmMarketTransactions)
            {
                CustomMessageBox.Show("Disable auto confirm trades to confirm your trades manual ");
                return;
            }

            if (o is not SteamGuardAccount selectedAccount)
                throw new ArgumentNullException(nameof(selectedAccount));

            Dictionary<SteamGuardAccount, List<ConfirmationModel>> confirmations = new();

            try
            {
                try
                {
                    ConfirmationModel[] tmp = await selectedAccount.FetchConfirmationsAsync();
                    foreach (var confirmationModel in tmp)
                    {
                        if (confirmationModel.ConfType == ConfirmationModel.ConfirmationType.Trade)
                        {

                        }

                        if (!confirmations.ContainsKey(selectedAccount))
                            confirmations[selectedAccount] = new List<ConfirmationModel>();

                        confirmations[selectedAccount].Add(confirmationModel);
                    }
                }
                catch (SteamGuardAccount.WgTokenInvalidException)
                {
                    await selectedAccount.RefreshSessionAsync();
                }
                catch (SteamGuardAccount.WgTokenExpiredException)
                {
                    //Prompt to relogin
                    ShowLoginWindow(LoginType.Refresh);
                }
                catch (WebException)
                {

                }

                foreach (var account in confirmations.Keys)
                {
                    foreach (var confirmation in confirmations[account])
                    {
                        ConfirmationViewModel model = new(account, confirmation);
                        model.OnCloseEvent += ModelOnOnCloseEvent;

                        Confirmations.Add(model);
                    }
                }
            }
            catch (SteamGuardAccount.WgTokenInvalidException)
            {

            }
        });

        #endregion

        #region PrivateMethods

        private void ModelOnOnCloseEvent(object? sender, EventArgs e)
        {
            if (sender is not ConfirmationViewModel model)
                throw new ArgumentException();

            Confirmations.Remove(model);
        }

        private void ShowLoginWindow(LoginType type)
        {
            LoginWindowView window = new();
            var dataContext = (window.DataContext as LoginWindowViewModel)!;
            dataContext.Account = SelectedAccount; //-V3149
            dataContext.LoginType = type;

            window.ShowDialog();
        }

        #endregion
    }
}
