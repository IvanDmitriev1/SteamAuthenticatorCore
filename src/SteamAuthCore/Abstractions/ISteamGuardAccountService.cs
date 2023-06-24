namespace SteamAuthCore.Abstractions;

public interface ISteamGuardAccountService
{
    Task<LoginAgainData> LoginAgain(SteamGuardAccount account, string password, LoginAgainData loginAgainData, CancellationToken cancellationToken);
    Task<IReadOnlyList<Confirmation>> FetchConfirmations(SteamGuardAccount account, CancellationToken cancellationToken);
    Task<bool> SendConfirmation(SteamGuardAccount account, Confirmation confirmation, ConfirmationOptions options, CancellationToken cancellationToken);
    Task<bool> SendConfirmation(SteamGuardAccount account, IReadOnlyList<Confirmation> confirmations, ConfirmationOptions options, CancellationToken cancellationToken);
}
