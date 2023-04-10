namespace SteamAuthenticatorCore.Shared.Abstractions;

public interface IConfirmationService
{
    Task Initialize();
    Task<IReadOnlyList<SteamGuardAccountConfirmationsModel>> CheckConfirmationFromAllAccounts();
}
