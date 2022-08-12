using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Options;

namespace SteamAuthenticatorCore.Desktop.Services;

public class UpdateServiceOptions
{
    public string GitHubUrl { get; set; } = string.Empty;
}

public sealed class UpdateService : IDisposable
{
    public UpdateService(IOptions<UpdateServiceOptions> options)
    {
        _gitHubUrl = options.Value.GitHubUrl;

        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "User");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }
    
    #region Models
    
    private class Asset
    {
        [JsonPropertyName("browser_download_url")]
        public string BrowserDownloadUrl { get; init; } = string.Empty;
    
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;
    }
    
    private class GitHubRequestApiModel
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; } = string.Empty;
    
        [JsonPropertyName("assets")]
        public Asset[] Assets { get; init; } = Array.Empty<Asset>();
    
        [JsonPropertyName("prerelease")]
        public bool Prerelease { get; init; }
    }
    
    public class CheckForUpdateModel
    {
        public CheckForUpdateModel(string appFileName, string downloadUrl, Version newVersion)
        {
            AppFileName = appFileName;
            DownloadUrl = downloadUrl;
            NewVersion = newVersion;
        }
    
        public bool NeedUpdate { get; set; }
    
        public string AppFileName { get; }
    
        public string DownloadUrl { get; }
    
        public Version NewVersion { get; }
    }
    
    #endregion
    
    private readonly string _gitHubUrl;
    private readonly HttpClient _httpClient;
    
    /// <summary>
    /// Checks releases for new version
    /// </summary>
    /// <param name="appFileNameToDownload"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <returns></returns>
    public async Task<CheckForUpdateModel> CheckForUpdate(string appFileNameToDownload)
    {
        if (await _httpClient.GetFromJsonAsync<GitHubRequestApiModel>(_gitHubUrl) is not { } datApiModel)
            throw new ArgumentException("Failed to check for update");
    
        if (datApiModel.TagName.Contains("v") || datApiModel.TagName.Contains("V"))
            datApiModel.TagName = datApiModel.TagName.Remove(0, 1);
    
        if (datApiModel.TagName.Contains("RC"))
            return new CheckForUpdateModel(string.Empty, string.Empty, new Version(0, 0));
    
        string downloadUrl = string.Empty;
        foreach (var asset in datApiModel.Assets)
        {
            if (asset.Name == appFileNameToDownload)
                downloadUrl = asset.BrowserDownloadUrl;
        }
    
        if (string.IsNullOrEmpty(downloadUrl))
            throw new ArgumentException($"Cannot find {appFileNameToDownload} on github");
    
        Version newVersion = new(datApiModel.TagName);
        Version currentVersion = new(FileVersionInfo.GetVersionInfo(Process.GetCurrentProcess().MainModule?.FileName!).FileVersion!);
    
        CheckForUpdateModel updateModel = new CheckForUpdateModel(appFileNameToDownload, downloadUrl, newVersion);
    
        switch (newVersion.CompareTo(currentVersion))
        {
            case 0:
            case < 0:
                return updateModel;
            case > 0:
                updateModel.NeedUpdate = true;
                return updateModel;
        }
    }
    
    /// <summary>
    /// Downloads and installs a new exe 
    /// </summary>
    /// <param name="model"></param>
    /// <returns>A new file name</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<string?> DownloadAndInstall(CheckForUpdateModel model)
    {
        string newFileName = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName!)!, $"{model.AppFileName.Remove(model.AppFileName.Length - 4, 4)}-{model.NewVersion}.exe");
    
        Stream stream;
        try
        {
            stream = await _httpClient.GetStreamAsync(model.DownloadUrl);
        }
        catch
        {
            MessageBox.Show("Failed to download update");
            return null;
        }
    
        await using (FileStream fileStream = new(newFileName, FileMode.Create))
            await stream.CopyToAsync(fileStream);
    
        await stream.DisposeAsync();
        return newFileName;
    }
    
    /// <summary>
    /// To delete the previous application file after the update
    /// </summary>
    /// <param name="appFileName">without .exe</param>
    /// <returns></returns>
    public async Task DeletePreviousFile(string appFileName)
    {
        await Task.Delay(3000);
    
        string[] files = Directory.GetFiles(Environment.CurrentDirectory);
        foreach (var file in files)
        {
            string fileName = Path.GetFileName(file);
    
            if (!fileName.Contains(appFileName) || !fileName.Contains("exe")) continue;
            if (fileName == Path.GetFileName(Process.GetCurrentProcess().MainModule?.FileName)) continue;
    
            try
            {
                File.Delete(file);
            }
            catch
            {
                Debug.WriteLine($"Failed to delete file in{nameof(DeletePreviousFile)} method");
            }
        }
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}