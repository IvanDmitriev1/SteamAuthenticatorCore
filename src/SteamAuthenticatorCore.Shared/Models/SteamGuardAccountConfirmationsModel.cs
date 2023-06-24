namespace SteamAuthenticatorCore.Shared.Models;

public sealed class SteamGuardAccountConfirmationsModel
{
    public SteamGuardAccountConfirmationsModel(SteamGuardAccount account, IEnumerable<Confirmation> confirmations)
    {
        Account = account;
        Confirmations = new ObservableCollection<Confirmation>(confirmations);
    }

    public SteamGuardAccount Account { get; }
    public ObservableCollection<Confirmation> Confirmations { get; }
}
