using System.Collections.Generic;
using SteamAuthCore.Models;
using System.Collections.ObjectModel;

namespace SteamAuthenticatorCore.Shared.Models;

public sealed class ConfirmationModel
{
    public ConfirmationModel(SteamGuardAccount account, List<SteamAuthCore.Models.ConfirmationModel> confirmations)
    {
        Account = account;
        Confirmations = new ObservableCollection<SteamAuthCore.Models.ConfirmationModel>(confirmations);
    }

    public SteamGuardAccount Account { get; }
    public ObservableCollection<SteamAuthCore.Models.ConfirmationModel> Confirmations { get; }
}
