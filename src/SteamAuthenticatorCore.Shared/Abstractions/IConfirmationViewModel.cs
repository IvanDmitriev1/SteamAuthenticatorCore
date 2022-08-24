using System.Collections.ObjectModel;
using System.Windows.Input;
using SteamAuthCore;
using SteamAuthCore.Models;

namespace SteamAuthenticatorCore.Shared.Abstractions;

public interface IConfirmationViewModel
{
    SteamGuardAccount Account { get; }
    ObservableCollection<ConfirmationModel> Confirmations { get; }

    ICommand ConfirmCommand { get; }
    ICommand CancelCommand { get; }
}
