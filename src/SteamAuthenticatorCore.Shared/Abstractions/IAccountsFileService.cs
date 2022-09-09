using System.IO;
using System.Threading.Tasks;
using SteamAuthCore.Models;

namespace SteamAuthenticatorCore.Shared.Abstractions;

public interface IAccountsFileService
{
    public const string AccountFileExtension = ".maFile";

    ValueTask InitializeOrRefreshAccounts();
    ValueTask<bool> SaveAccount(Stream stream, string fileName);
    ValueTask SaveAccount(SteamGuardAccount account);
    ValueTask DeleteAccount(SteamGuardAccount accountToRemove);
}