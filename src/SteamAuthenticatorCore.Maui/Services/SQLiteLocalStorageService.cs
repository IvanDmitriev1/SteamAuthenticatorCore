using SQLite;
using SteamAuthCore.Models;
using SteamAuthenticatorCore.Mobile.Data;
using SteamAuthenticatorCore.Mobile.Extensions;
using SteamAuthenticatorCore.Shared.Abstractions;
using System.Text.Json;

namespace SteamAuthenticatorCore.Mobile.Services;

internal class SqLiteLocalStorageService : IAccountsService
{
    private readonly SemaphoreSlim _semaphoreSlim = new(0);
    private SQLiteAsyncConnection? _connection;
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

        _semaphoreSlim.Release();
    }

    public async ValueTask<IReadOnlyList<SteamGuardAccount>> GetAll()
    {
        if (_connection is null)
            await _semaphoreSlim.WaitAsync().ConfigureAwait(false);

        var dtoArray = await _connection!.Table<SteamGuardAccountDto>().ToArrayAsync().ConfigureAwait(false);
        var accounts = new SteamGuardAccount[dtoArray.Length];

        for (var i = 0; i < dtoArray.Length; i++)
        {
            var dto = dtoArray[i];
            var sessionDto = await _connection!.GetAsync<SessionDataDto>(dto.SessionId).ConfigureAwait(false);

            accounts[i] = dto.MapFromDto(sessionDto);
        }

        return accounts;
    }

    public async ValueTask<bool> Save(Stream stream, string fileName)
    {
        if (_connection is null)
            await _semaphoreSlim.WaitAsync().ConfigureAwait(false);

        if (await JsonSerializer.DeserializeAsync<SteamGuardAccount>(stream).ConfigureAwait(false) is not { } account)
            return false;

        var sessionId = await _connection!.InsertAsync(account.Session.MapToDto()).ConfigureAwait(false);
        var accountDto = account.MapToDto(sessionId);

        await _connection!.InsertAsync(accountDto).ConfigureAwait(false);

        return true;
    }

    public async ValueTask Update(SteamGuardAccount account)
    {
        if (_connection is null)
            await _semaphoreSlim.WaitAsync().ConfigureAwait(false);

        var dto = await _connection!.Table<SteamGuardAccountDto>().FirstOrDefaultAsync(dto =>
            dto.SharedSecret == account.SharedSecret && dto.IdentitySecret == account.IdentitySecret);

        if (dto is null)
            return;

        var newDto = account.MapToDto(dto.SessionId);
        newDto.Id = dto.Id;

        var result = await _connection.UpdateAsync(newDto);
    }

    public async ValueTask Delete(SteamGuardAccount account)
    {
        if (_connection is null)
            await _semaphoreSlim.WaitAsync().ConfigureAwait(false);

        var dto = await _connection!.Table<SteamGuardAccountDto>().FirstOrDefaultAsync(dto =>
            dto.SharedSecret == account.SharedSecret && dto.IdentitySecret == account.IdentitySecret);

        if (dto is null)
            return;

        var result = await _connection.DeleteAsync<SteamGuardAccountDto>(dto.Id);
    }
}