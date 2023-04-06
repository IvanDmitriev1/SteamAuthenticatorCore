namespace SteamAuthenticatorCore.Shared.Abstractions;

public interface IAccountsService
{
    public const string AccountFileExtension = ".maFile";

    Task Initialize();
    ValueTask<IReadOnlyList<SteamGuardAccount>> GetAll();
    ValueTask<bool> Save(Stream stream, string fileName);
    ValueTask Update(SteamGuardAccount account);
    ValueTask Delete(SteamGuardAccount account);
}
