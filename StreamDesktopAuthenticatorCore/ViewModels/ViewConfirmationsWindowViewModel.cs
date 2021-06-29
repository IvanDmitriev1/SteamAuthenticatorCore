using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using SteamAuthCore.Models;
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

        public ObservableCollection<ConfirmationViewModel> Confirmations { get; set; }

        public SteamGuardAccount? SelectedAccount
        {
            get => _selectedAccount;
            set => Set(ref _selectedAccount, value);
        }

        #endregion

        #region Commands

        public ICommand UpdateCommand => new AsyncRelayCommand(async o => await UpdateTrades());

        #endregion

        #region PrivateMethods

        private async Task UpdateTrades()
        {
            if (SelectedAccount is null)
                throw new ArgumentNullException(nameof(SelectedAccount));

            SteamGuardAccount[] accs = _manifest.CheckAllAccounts ? _manifest.Accounts.ToArray() : new[] { SelectedAccount };
            Dictionary<SteamGuardAccount, List<ConfirmationModel>> confirmations = new();

            try
            {
                foreach (var acc in accs)
                {
                    try
                    {
                        ConfirmationModel[] tmp = await acc.FetchConfirmationsAsync();
                        foreach (var conf in tmp)
                        {
                            if (!confirmations.ContainsKey(acc))
                                confirmations[acc] = new List<ConfirmationModel>();

                            confirmations[acc].Add(conf);
                        }
                    }
                    catch (SteamGuardAccount.WgTokenInvalidException)
                    {
                        await acc.RefreshSessionAsync();
                    }
                    catch (SteamGuardAccount.WgTokenExpiredException)
                    {
                        //Prompt to relogin
                        ShowLoginWindow(LoginType.Refresh);
                    }
                    catch (WebException)
                    {

                    }
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
        }

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
