using SQLite;
using System.Text.Json;

namespace SteamAuthenticatorCore.Mobile.Services;

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

        try
        {
            if (await JsonSerializer.DeserializeAsync<SteamGuardAccount>(stream).ConfigureAwait(false) is not { } account)
                return false;

            var sessionId = await _connection.InsertAsync(account.Session.MapToDto()).ConfigureAwait(false);
            var accountDto = account.MapToDto(sessionId);

            await _connection.InsertAsync(accountDto).ConfigureAwait(false);
            _accounts.Add(account);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async ValueTask Update(SteamGuardAccount account)
    {
        if (!_isInitialized)
            await Initialize();

        var dto = await _connection.Table<SteamGuardAccountDto>().FirstOrDefaultAsync(dto =>
            dto.SharedSecret == account.SharedSecret && dto.IdentitySecret == account.IdentitySecret);

        if (dto is null)
            return;

        var newDto = account.MapToDto(dto.SessionId);
        newDto.Id = dto.Id;

        var result = await _connection.UpdateAsync(newDto);
    }

    public async ValueTask Delete(SteamGuardAccount account)
    {
        if (!_isInitialized)
            await Initialize();

        var dto = await _connection.Table<SteamGuardAccountDto>().FirstOrDefaultAsync(dto =>
            dto.SharedSecret == account.SharedSecret && dto.IdentitySecret == account.IdentitySecret);

        if (dto is null)
            return;

        var result = await _connection.DeleteAsync<SteamGuardAccountDto>(dto.Id);
        _accounts.Remove(account);
    }
}