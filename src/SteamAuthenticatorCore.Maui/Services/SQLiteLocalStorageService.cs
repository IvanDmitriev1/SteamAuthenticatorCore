using System.Text.Json;
using SQLite;

namespace SteamAuthenticatorCore.Maui.Services;

internal class SqLiteLocalStorageService : IAccountsService
{
    private readonly List<SteamGuardAccount> _accounts = new();
    private SQLiteAsyncConnection _connection = null!;
    private bool _isInitialized;

    public async Task Initialize()
    {
        if (_isInitialized)
            return;

        _isInitialized = true;

        var databasePath = Path.Combine(FileSystem.AppDataDirectory, "SteamGuardAccounts.db");
        const SQLiteOpenFlags flags = SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache;

        _connection = new SQLiteAsyncConnection(databasePath, flags);
        var result = await _connection.CreateTablesAsync<SessionDataDto, SteamGuardAccountDto>().ConfigureAwait(false);
    }

    public async ValueTask<IReadOnlyList<SteamGuardAccount>> GetAll()
    {
        if (!_isInitialized)
            await Initialize();

        if (_accounts.Any())
            return _accounts.AsReadOnly();

        var dtoArray = await _connection.Table<SteamGuardAccountDto>().ToArrayAsync().ConfigureAwait(false);

        foreach (var dto in dtoArray)
        {
            var sessionDto = await _connection.GetAsync<SessionDataDto>(dto.SessionId).ConfigureAwait(false);
            _accounts.Add(dto.MapFromDto(sessionDto));
        }

        return _accounts.AsReadOnly();
    }

    public async ValueTask<bool> Save(Stream stream, string fileName)
    {
        if (!_isInitialized)
            await Initialize();

        if (await JsonSerializer.DeserializeAsync<SteamGuardAccount>(stream).ConfigureAwait(false) is not { } account)
            return false;

        if (await _connection.Table<SteamGuardAccountDto>().FirstOrDefaultAsync(dto =>
                dto.SerialNumber == account.SerialNumber && dto.RevocationCode == account.RevocationCode &&
                dto.AccountName == account.AccountName && dto.IdentitySecret == account.IdentitySecret &&
                dto.Secret1 == account.Secret1) is not null)
            return false;

        var sessionId = await _connection.InsertAsync(account.Session.MapToDto()).ConfigureAwait(false);
        var accountDto = account.MapToDto(sessionId);

        await _connection.InsertAsync(accountDto).ConfigureAwait(false);
        _accounts.Add(account);

        return true;
    }

    public async ValueTask Update(SteamGuardAccount account)
    {
        if (!_isInitialized)
            await Initialize();

        if (await _connection.Table<SteamGuardAccountDto>().FirstOrDefaultAsync(dto =>
                dto.SerialNumber == account.SerialNumber && dto.RevocationCode == account.RevocationCode &&
                dto.AccountName == account.AccountName && dto.IdentitySecret == account.IdentitySecret &&
                dto.Secret1 == account.Secret1) is not { } dtoObj)
            return;

        var sessionNewDto = account.Session.MapToDto();
        sessionNewDto.Id = dtoObj.SessionId;

        var newDto = account.MapToDto(dtoObj.SessionId);
        newDto.Id = dtoObj.Id;

        var sessionResult = await _connection.UpdateAsync(sessionNewDto);
        var accountResult = await _connection.UpdateAsync(newDto);
    }

    public async ValueTask Delete(SteamGuardAccount account)
    {
        if (!_isInitialized)
            await Initialize();

        if (await _connection.Table<SteamGuardAccountDto>().FirstOrDefaultAsync(dto =>
                dto.SerialNumber == account.SerialNumber && dto.RevocationCode == account.RevocationCode &&
                dto.AccountName == account.AccountName && dto.IdentitySecret == account.IdentitySecret &&
                dto.Secret1 == account.Secret1) is not { } dtoObj)
            return;

        var result = await _connection.DeleteAsync<SteamGuardAccountDto>(dtoObj.Id);
        _accounts.Remove(account);
    }
}