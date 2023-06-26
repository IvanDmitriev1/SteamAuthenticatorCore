using System.IO;
using System.Text;
using System.Text.Json;

namespace SteamAuthenticatorCore.Desktop.Services;

internal class LocalDriveAccountsService : IAccountsService
{
    public LocalDriveAccountsService(IPlatformImplementations platformImplementations, ILogger<LocalDriveAccountsService> logger)
    {
        _accounts = new List<SteamGuardAccount>();

        _platformImplementations = platformImplementations;
        _logger = logger;
        _maFilesDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SteamAuthenticatorCore.Desktop");
    }

    private readonly List<SteamGuardAccount> _accounts;
    private readonly IPlatformImplementations _platformImplementations;
    private readonly ILogger<LocalDriveAccountsService> _logger;
    private readonly string _maFilesDirectory;
    private bool _isInitialized = false;

    public async Task Initialize()
    {
        if (_isInitialized)
            return;

        _isInitialized = true;

        CreateDirectory();

        _accounts.Clear();

        foreach (var filePath in Directory.GetFiles(_maFilesDirectory))
        {
            if (!filePath.Contains(IAccountsService.AccountFileExtension))
                continue;

            try
            {
                await using var fileStream = File.OpenRead(filePath);

                if (await JsonSerializer.DeserializeAsync<SteamGuardAccount>(fileStream) is not { } account)
                    continue;

                _accounts.Add(account);
            }
            catch(Exception e)
            {
                _logger.LogCritical(e, "Exception when loading accounts");
                await _platformImplementations.DisplayAlert("Error", $"Failed to load account at {filePath}");
            }
        }
    }

    public ValueTask<IReadOnlyList<SteamGuardAccount>> GetAll()
    {
        IReadOnlyList<SteamGuardAccount> wrapper = _accounts.AsReadOnly();

        return ValueTask.FromResult(wrapper);
    }

    public async ValueTask<bool> Save(Stream stream, string fileName)
    {
        if (await JsonSerializer.DeserializeAsync<SteamGuardAccount>(stream) is not { } account)
            return false;

        if (FindAccountInDirectory(account) is not null)
            return false;

        var newFileName = Path.ChangeExtension(Path.GetFileNameWithoutExtension(fileName), IAccountsService.AccountFileExtension);
        var newFilePath = Path.Combine(_maFilesDirectory, newFileName);

        stream.Seek(0, SeekOrigin.Begin);
        await using var newFileStream = File.OpenWrite(newFilePath);
        await stream.CopyToAsync(newFileStream);

        _accounts.Add(account);
        return true;
    }

    public async ValueTask Update(SteamGuardAccount account)
    {
        if (FindAccountInDirectory(account) is not { } filePath)
            return;

        var json = JsonSerializer.Serialize(account);
        await using var streamWriter = new StreamWriter(filePath, Encoding.UTF8, new FileStreamOptions()
        {
            Access = FileAccess.Write,
            Mode = FileMode.Create,
            Share = FileShare.None,
        });
        await streamWriter.WriteAsync(json);
    }

    public ValueTask Delete(SteamGuardAccount account)
    {
        if (FindAccountInDirectory(account) is not { } filePath)
            return ValueTask.CompletedTask;

        File.Delete(filePath);
        _accounts.Remove(account);

        return ValueTask.CompletedTask;
    }

    private void CreateDirectory()
    {
        if (!Directory.Exists(_maFilesDirectory))
            Directory.CreateDirectory(_maFilesDirectory);
    }

    private string? FindAccountInDirectory(SteamGuardAccount accountToFind)
    {
        foreach (var filePath in Directory.GetFiles(_maFilesDirectory))
        {
            if (!filePath.Contains(IAccountsService.AccountFileExtension))
                continue;

            using var fileStream = File.OpenRead(filePath);

            if (JsonSerializer.Deserialize<SteamGuardAccount>(fileStream) is not { } account)
                continue;

            if (account != accountToFind)
                continue;

            return filePath;
        }

        return null;
    }
}
