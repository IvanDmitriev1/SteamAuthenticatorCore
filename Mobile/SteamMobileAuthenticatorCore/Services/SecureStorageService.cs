using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using SteamAuthCore;
using SteamAuthCore.Manifest;
using Xamarin.Essentials;

namespace SteamAuthenticatorCore.Mobile.Services
{
    public class SecureStorageService : IManifestModelService
    {
        public SecureStorageService()
        {
            _accountNames = new HashSet<string>();
        }

        private readonly HashSet<string> _accountNames;

        public Task Initialize(IManifestDirectoryService? directoryService = null)
        {
            var json = Preferences.Get("AccountsNames", string.Empty);
            if (string.IsNullOrEmpty(json))
                return Task.CompletedTask;

            foreach (var val in JsonSerializer.Deserialize<string[]>(json) ?? Array.Empty<string>())
            {
                _accountNames.Add(val);
            };

            return Task.CompletedTask;
        }

        public ManifestModel GetManifestModel()
        {
            throw new NotImplementedException();
        }

        public Task SaveManifest()
        {
            return Task.CompletedTask;
        }

        public async Task<ICollection<SteamGuardAccount>> GetAccounts()
        {
            List<SteamGuardAccount> accounts = new List<SteamGuardAccount>();

            foreach (var key in _accountNames)
            {
                if (await GetForSecureStorage(key) is not { } account)
                    continue;

                accounts.Add(account);
            }
            return accounts;
        }

        public async Task<SteamGuardAccount?> AddSteamGuardAccount(Stream fileStream, string fileName)
        {
            var account = await JsonSerializer.DeserializeAsync<SteamGuardAccount>(fileStream);

            if (account == null) 
                return account;

            if (_accountNames.Contains(account.AccountName))
                return null;

            await fileStream.FlushAsync();
            fileStream.Position = 0;
            using var streamReader = new StreamReader(fileStream);

            var json = await streamReader.ReadToEndAsync();
            await SaveToSecureStorage(account.AccountName, json);
            SaveAccountNames();

            return account;
        }

        public async Task SaveSteamGuardAccount(SteamGuardAccount account)
        {
            if (_accountNames.Contains(account.AccountName))
                return;

            var json = JsonSerializer.Serialize(account);
            await SaveToSecureStorage(account.AccountName, json);

            SaveAccountNames();
        }

        public Task DeleteSteamGuardAccount(SteamGuardAccount account)
        {
            _accountNames.Remove(account.AccountName);
            SecureStorage.Remove(account.AccountName);
            SaveAccountNames();

            return Task.CompletedTask;
        }

        private async Task<SteamGuardAccount?> GetForSecureStorage(string key)
        {
            try
            {
                if (JsonSerializer.Deserialize<SteamGuardAccount>(await SecureStorage.GetAsync(key)) is { } account)
                    return account;

                SecureStorage.Remove(key);
                _accountNames.Remove(key);

                return null;

            }
            catch (Exception)
            {
                SecureStorage.Remove(key);
                _accountNames.Remove(key);

                return null;
            }
        }

        private Task SaveToSecureStorage(string key, string json)
        {
            _accountNames.Add(key);
            return SecureStorage.SetAsync(key, json);
        }

        private void SaveAccountNames()
        {
            var arr = _accountNames.ToArray();
            Preferences.Set("AccountsNames", JsonSerializer.Serialize(arr));
        }
    }
}
