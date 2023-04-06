using Octokit;
using SteamAuthenticatorCore.Maui.MyPermissions;
using System.Net.Http.Headers;
using System.Reflection;
using Application = Microsoft.Maui.Controls.Application;
using FileMode = System.IO.FileMode;

namespace SteamAuthenticatorCore.Maui.Popups;

public partial class UpdatePopup
{
    public UpdatePopup(Release release)
	{
        _release = release;
        InitializeComponent();

        var releaseAssets = release.FindSuitableAssets(".apk");
        if (releaseAssets.Length < 1)
        {
            Close();
            return;
        }

        _releaseAsset = releaseAssets[0];
        BindingContext = release;

        _progress = new Progress<double>(ProgressHandler);
    }

    static UpdatePopup()
    {
        HttpClient = new HttpClient();
        HttpClient.DefaultRequestHeaders.Add("User-Agent", "User");
        HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private static readonly HttpClient HttpClient;

    private readonly ReleaseAsset _releaseAsset = null!;
    private readonly Release _release;
    private readonly IProgress<double> _progress = null!;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private void UpdatePopup_OnClosed(object? sender, PopupClosedEventArgs e)
    {
        _cancellationTokenSource.Dispose();
    }

    private async void OnUpdateClicked(object? sender, EventArgs e)
    {
        CanBeDismissedByTappingOutsideOfPopup = false;
        ProgressBar.IsVisible = true;
        UpdateButton.IsVisible = false;

        var version = _release.TagName.TrimStart('v');
        var newVersion = new Version(version);

        if (!await GrandPermissions())
        {
            Close();
            return;
        }

        await DownloadAndInstall(_releaseAsset, newVersion);
    }

    private void OnCancelClicked(object? sender, EventArgs e)
    {
        _cancellationTokenSource.Cancel();

        Close();
    }

    private async Task DownloadAndInstall(ReleaseAsset releaseAsset, Version version)
    {
        var downloadPath = Path.Combine(FileSystem.Current.AppDataDirectory, "Downloads");
        if (!Directory.Exists(downloadPath))
            Directory.CreateDirectory(downloadPath);

        try
        {
            var filePath = Path.Combine(downloadPath, $"{AppInfo.PackageName}-{version}.apk");
            if (File.Exists(filePath))
            {
                AppInstaller.Install(filePath);
                return;
            }

            await using var stream = File.Create(filePath);

            await Toast.Make($"Download started").Show();
            await HttpClient.DownloadAsync(releaseAsset.BrowserDownloadUrl, stream, _progress, _cancellationTokenSource.Token);

            AppInstaller.Install(filePath);
        }
        catch (TaskCanceledException)
        {
            
        }
    }

    private void ProgressHandler(double obj)
    {
        var progress = obj / 100.0;

        ProgressBar.Progress = progress;
    }

    private static async Task<bool> GrandPermissions()
    {
        var status = await Permissions.CheckStatusAsync<InstallPackagesPermission>();
        if (status == PermissionStatus.Granted)
            return true;

        var result = await Permissions.RequestAsync<InstallPackagesPermission>();

        return result == PermissionStatus.Granted;
    }
}