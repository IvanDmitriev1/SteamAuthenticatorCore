namespace SteamAuthCore.Abstractions;

public interface ISteamGuardAccountService
{
    Task<IEnumerable<ConfirmationModel>> FetchConfirmations(SteamGuardAccount account, CancellationToken cancellationToken);
    Task<bool> SendConfirmation(SteamGuardAccount account, ConfirmationModel confirmation, ConfirmationOptions options, CancellationToken cancellationToken);
    Task<bool> SendConfirmation(SteamGuardAccount account, ConfirmationModel[] confirmations, ConfirmationOptions options, CancellationToken cancellationToken);
}
