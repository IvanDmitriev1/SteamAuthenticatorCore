using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using Octokit;
using FileMode = System.IO.FileMode;

namespace SteamAuthenticatorCore.Desktop.Dialogs;

public partial class DownloadUpdateContentDialog 
{
    public DownloadUpdateContentDialog(ContentPresenter contentPresenter, Release release) : base(contentPresenter)
    {
        InitializeComponent();
        DataContext = release;

        var releaseAssets = release.FindSuitableAssets(".exe");

        if (releaseAssets.Length > 1)
        {
            ListView.ItemsSource = releaseAssets;
            ListViewGrid.Visibility = Visibility.Visible;
        }
        else
        {
            _asset = releaseAssets[0];
        }

        _cancellationTokenSource = new CancellationTokenSource();
        _progress = new Progress<double>(ProgressHandler);
    }

    static DownloadUpdateContentDialog()
    {
        HttpClient = new HttpClient();
        HttpClient.DefaultRequestHeaders.Add("User-Agent", "User");
        HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private static readonly HttpClient HttpClient;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly ReleaseAsset? _asset;
    private readonly IProgress<double> _progress;

    protected override bool OnButtonClick(ContentDialogButton button)
    {
        if (button == ContentDialogButton.Close)
        {
            _cancellationTokenSource.Cancel();
            return true;
        }

        if (ListViewGrid.Visibility == Visibility.Collapsed && _asset is not null)
        {
            DownloadAndInstall(_asset);
            return false;
        }

        if (ListViewGrid.Visibility == Visibility.Visible && ListView.SelectedItem is ReleaseAsset selectedRelease)
        {
            DownloadAndInstall(selectedRelease);
            return false;
        }

        return false;
    }

    private void ProgressHandler(double obj)
    {
        ProgressBar.Value = obj;
    }

    private async void DownloadAndInstall(ReleaseAsset asset)
    {
        IsPrimaryButtonEnabled = false;
        ProgressBarSnackPanel.Visibility = Visibility.Visible;
        ProgressBar.Focus();

        string newFileName = $"download{Guid.NewGuid()}.exe";
        string newFilePath = Path.Combine(Directory.GetCurrentDirectory(), newFileName);

        try
        {
            using var response = await HttpClient.GetAsync(asset.BrowserDownloadUrl, HttpCompletionOption.ResponseHeadersRead, _cancellationTokenSource.Token);

            await using var fileStream = new FileStream(newFilePath, FileMode.OpenOrCreate);
            await response.DownloadAsync(fileStream, _progress, _cancellationTokenSource.Token);

            var currentExeName = AppDomain.CurrentDomain.FriendlyName + ".exe";
            var currentExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, currentExeName);

            var commandLineBuilder = new StringBuilder();
            commandLineBuilder.Append($"taskkill /f /im \"{currentExeName}\"");
            commandLineBuilder.Append(" && ");
            commandLineBuilder.Append("timeout /t 1");
            commandLineBuilder.Append(" && ");
            commandLineBuilder.Append($"del \"{currentExePath}\"");
            commandLineBuilder.Append(" && ");
            commandLineBuilder.Append($"ren \"{newFileName}\" \"{currentExeName}\"");
            commandLineBuilder.Append(" && ");
            commandLineBuilder.Append($"\"{currentExePath}\"");
            Cmd(commandLineBuilder.ToString());
        }
        catch (TaskCanceledException)
        {
            File.Delete(newFilePath);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }

    private static void Cmd(string line)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "cmd",
            Arguments = $"/c {line}",
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Hidden
        });
    }
}