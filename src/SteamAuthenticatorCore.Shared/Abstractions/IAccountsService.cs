using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SteamAuthCore.Models;

namespace SteamAuthenticatorCore.Shared.Abstractions;

public interface IAccountsService
{
    public const string AccountFileExtension = ".maFile";

    Task Initialize();
    ValueTask<IReadOnlyList<SteamGuardAccount>> GetAll();
    ValueTask<bool> Save(Stream stream, string fileName);
    ValueTask Save(SteamGuardAccount account);
    ValueTask Delete(SteamGuardAccount account);
}
