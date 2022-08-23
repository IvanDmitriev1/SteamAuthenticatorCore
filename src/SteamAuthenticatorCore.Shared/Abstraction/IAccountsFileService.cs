using System.IO;
using System.Threading.Tasks;
using SteamAuthCore;

namespace SteamAuthenticatorCore.Shared.Abstraction;

public interface IAccountsFileService
{
    public const string AccountFileExtension = ".maFile";

    ValueTask InitializeOrRefreshAccounts();
    ValueTask<bool> SaveAccount(Stream stream, string fileName);
    ValueTask SaveAccount(SteamGuardAccount account);
    ValueTask DeleteAccount(SteamGuardAccount accountToRemove);
}