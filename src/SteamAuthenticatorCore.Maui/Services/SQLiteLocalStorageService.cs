using SteamAuthCore.Models;
using SteamAuthenticatorCore.Shared.Abstractions;

namespace SteamAuthenticatorCore.Mobile.Services;

internal class SqLiteLocalStorageService : IAccountsService
{
    public Task Initialize()
    {
        throw new NotImplementedException();
    }

    public ValueTask<IReadOnlyList<SteamGuardAccount>> GetAll()
    {
        throw new NotImplementedException();
    }

    public ValueTask<bool> Save(Stream stream, string fileName)
    {
        throw new NotImplementedException();
    }

    public ValueTask Save(SteamGuardAccount account)
    {
        throw new NotImplementedException();
    }

    public ValueTask Delete(SteamGuardAccount account)
    {
        throw new NotImplementedException();
    }
}