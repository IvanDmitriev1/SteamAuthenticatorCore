using System;
using System.Windows.Input;
using SteamAuthCore;
using WpfHelper;
using WpfHelper.Commands;

namespace SteamDesktopAuthenticatorCore.ViewModels
{
    class ConfirmationViewModel : BaseViewModel
    {
        public ConfirmationViewModel(in SteamGuardAccount selectedAccount, in ConfirmationModel confirmation )
        {
            Confirmation = confirmation;
            SelectedAccount = selectedAccount;
        }

        #region Event

        public event EventHandler? OnCloseEvent;

        private void OnOnCloseEvent()
        {
            OnCloseEvent?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Variabels

        private string _statusText = string.Empty;
        private bool _acceptButtonClicked = false;
        private bool _denyButtonClicked = false;

        #endregion

        #region Fields

        public SteamGuardAccount SelectedAccount { get; }

        public ConfirmationModel Confirmation { get; }

        public string StatusText
        {
            get => _statusText;
            set => Set(ref _statusText, value);
        }

        #endregion

        #region Commands

        public ICommand AcceptButtonOnclickCommand => new RelayCommand(o =>
        {
            if (!_acceptButtonClicked)
            {
                StatusText = "Press Accept again to confirm";
                _acceptButtonClicked = true;
                return;
            }

            StatusText = "Accepting...";
            SelectedAccount.SendConfirmationAjax(Confirmation, SteamGuardAccount.Confirmation.Allow);
            OnOnCloseEvent();
        });

        public ICommand DenyButtonOnClickCommand => new RelayCommand(o =>
        {
            if (!_denyButtonClicked)
            {
                StatusText = "Press Deny again to confirm";
                _denyButtonClicked = true;
                return;   
            }

            StatusText = "Denying...";
            SelectedAccount.SendConfirmationAjax(Confirmation, SteamGuardAccount.Confirmation.Deny);
            OnOnCloseEvent();
        });

        #endregion
    }
}
