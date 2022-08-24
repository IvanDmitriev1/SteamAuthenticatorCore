using SteamAuthCore.Models;
using SteamAuthCore;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SteamAuthenticatorCore.Shared.Abstraction;

public interface IConfirmationViewModel
{
    SteamGuardAccount Account { get; }
    ObservableCollection<ConfirmationModel> Confirmations { get; }

    ICommand ConfirmCommand { get; }
    ICommand CancelCommand { get; }
}
