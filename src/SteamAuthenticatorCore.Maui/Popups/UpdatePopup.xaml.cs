using Octokit;
using System.Net.Http.Headers;

namespace SteamAuthenticatorCore.Maui.Popups;

public partial class UpdatePopup
{
    public UpdatePopup(Release release)
	{
        InitializeComponent();

        var releaseAssets = release.FindSuitableAssets(".apk");
        if (releaseAssets.Length < 1)
        {
            Close();
            return;
        }

        _release = release;
        _releaseAsset = releaseAssets[0];
        Label.Text = _release.Body;

        _progress = new Progress<double>(ProgressHandler);

        var deviseWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
        //var deviseHeight = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;

        Border.WidthRequest = deviseWidth / 1.2;
    }

    static UpdatePopup()
    {
        HttpClient = new HttpClient();
        HttpClient.DefaultRequestHeaders.Add("User-Agent", "User");
        HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private static readonly HttpClient HttpClient;

    private readonly ReleaseAsset _releaseAsset = null!;
    private readonly Release _release = null!;
    private readonly IProgress<double> _progress = null!;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private void UpdatePopup_OnClosed(object? sender, PopupClosedEventArgs e)
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }

    private async void OnUpdateClicked(object? sender, EventArgs e)
    {
        if (!await GrandPermissions())
        {
            await Toast.Make("Failed to request permissions", ToastDuration.Long).Show();
            Close();
            return;
        }

        CanBeDismissedByTappingOutsideOfPopup = false;
        ProgressBar.IsVisible = true;
        UpdateButton.IsVisible = false;

        var version = _release.TagName.TrimStart('v');
        var newVersion = new Version(version);

        await DownloadAndInstall(_releaseAsset, newVersion);
    }

    private void OnCancelClicked(object? sender, EventArgs e)
    {
        Close();
    }

    private async Task DownloadAndInstall(ReleaseAsset releaseAsset, Version version)
    {
        try
        {
            var fileName = $"{AppInfo.PackageName}-{version}.apk";
            var filePath = Path.Combine(AppInstaller.DownloadedDirectory, fileName);

            using var response = await HttpClient.GetAsync(releaseAsset.BrowserDownloadUrl, HttpCompletionOption.ResponseHeadersRead, _cancellationTokenSource.Token);
            var totalDownloadSize = response.Content.Headers.ContentLength;

            var fileInfo = new FileInfo(filePath);

            if (fileInfo.Exists && fileInfo.Length == totalDownloadSize)
            {
                ProgressBar.Progress = 1;
                AppInstaller.Install(fileName);
                return;
            }

            await using var stream = fileInfo.Create();

            await Toast.Make("Started downloading").Show();
            await response.DownloadAsync(stream, _progress,
                _cancellationTokenSource.Token);

            AppInstaller.Install(fileName);
        }
        catch (TaskCanceledException)
        {
            return;
        }
        catch
        {
            //
        }

        Close();
    }

    private void ProgressHandler(double obj)
    {
        var progress = obj / 100.0;

        ProgressBar.Progress = progress;
    }

    private static async Task<bool> GrandPermissions()
    {
        if (await Permissions.RequestAsync<Permissions.StorageWrite>() != PermissionStatus.Granted)
            return false;

        if (await Permissions.RequestAsync<Permissions.StorageRead>() != PermissionStatus.Granted)
            return false;

        /*if (await Permissions.RequestAsync<InstallPackagesPermission>() != PermissionStatus.Granted)
            return false;*/

        return true;
    }
}