using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SteamAuthCore;
using WpfHelper.Commands;
using BaseViewModel = WPFUI.Common.BaseViewModel;

namespace SteamDesktopAuthenticatorCore.ViewModels
{
    public class ConfirmationAccountViewModel : BaseViewModel
    {
        public ConfirmationAccountViewModel(SteamGuardAccount account, ConfirmationModel[] confirmations)
        {
            Account = account;

            foreach (var confirmation in confirmations)
                confirmation.BitMapImage = new BitmapImage(new Uri(confirmation.ImageSource, UriKind.Absolute));

            Confirmations = new ObservableCollection<ConfirmationModel>(confirmations);
        }

        public SteamGuardAccount Account { get; }
        public ObservableCollection<ConfirmationModel> Confirmations { get; }

        public ICommand AllowCommand => new RelayCommand( o =>
        {
            Task.Run(() =>
            {
                var list = (IList)o!;
                var confirmations = list.Cast<ConfirmationModel>().ToArray();

                Account.SendConfirmationAjax(confirmations, SteamGuardAccount.Confirmation.Allow);

                foreach (var confirmation in confirmations)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Confirmations.Remove(confirmation);
                    });
                }
            });
        });

        public ICommand DenyCommand => new AsyncRelayCommand( async o =>
        {
            await Task.Run(() =>
            {
                var list = (IList)o!;
                var confirmations = list.Cast<ConfirmationModel>().ToArray();

                Account.SendConfirmationAjax(confirmations, SteamGuardAccount.Confirmation.Deny);

                foreach (var confirmation in confirmations)
                {
                    var q = Account.GetConfirmationDetails(confirmation);


                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Confirmations.Remove(confirmation);
                    });
                }
            });
        });
    }

    public class ConfirmationViewModel : BaseViewModel
    {
        public ConfirmationViewModel(ObservableCollection<SteamGuardAccount> steamGuardAccounts)
        {
            _steamGuardAccounts = steamGuardAccounts;
            _tradeAutoConfirmationTimer = new DispatcherTimer();
            _tradeAutoConfirmationTimer.Tick += async (sender, args) => await TradeAutoConfirmationTimerOnTick();

            Accounts = new ObservableCollection<ConfirmationAccountViewModel>();
        }

        private readonly ObservableCollection<SteamGuardAccount> _steamGuardAccounts;
        private readonly DispatcherTimer _tradeAutoConfirmationTimer;

        public ObservableCollection<ConfirmationAccountViewModel> Accounts { get; }

        public void ChangeTradeAutoConfirmationTimerInterval(int interval, bool? startTimer = null)
        {
            _tradeAutoConfirmationTimer.Interval = TimeSpan.FromSeconds(interval);

            switch (startTimer)
            {
                case true:
                    _tradeAutoConfirmationTimer.Start();
                    break;
                case false:
                    _tradeAutoConfirmationTimer.Stop();
                    break;
            }
        }

        public ICommand CheckConfirmationsCommand => new AsyncRelayCommand(async o =>
        {
            await CheckConfirmations();
        });

        private async Task TradeAutoConfirmationTimerOnTick()
        {

        }

        private async Task CheckConfirmations()
        {
            Accounts.Clear();

            foreach (var account in _steamGuardAccounts)
            {
                try
                {
                    if (await CreateConfirmationAccountViewModel(account) is not { } confirmationAccountViewModel) continue;

                    Accounts.Add(confirmationAccountViewModel);
                }
                catch (SteamGuardAccount.WgTokenInvalidException)
                {
                    await account.RefreshSessionAsync();

                    try
                    {
                        if (await CreateConfirmationAccountViewModel(account) is not { } confirmationAccountViewModel) continue;

                        Accounts.Add(confirmationAccountViewModel);
                    }
                    catch (SteamGuardAccount.WgTokenInvalidException)
                    {
                    }
                }
                catch (SteamGuardAccount.WgTokenExpiredException)
                {

                }
            }
        }

        private static async Task<ConfirmationAccountViewModel?> CreateConfirmationAccountViewModel(SteamGuardAccount account)
        {
            var confirmations = (await account.FetchConfirmationsAsync()).ToArray();
            return confirmations.Length > 0 ? new ConfirmationAccountViewModel(account, confirmations) : null;
        }
    }
}
