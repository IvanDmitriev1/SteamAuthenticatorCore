using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SteamAuthenticatorCore.Desktop.Helpers;
using SteamAuthenticatorCore.Shared.Models;
using SteamAuthenticatorCore.Shared.Services;

namespace SteamAuthenticatorCore.Desktop.Services;

internal class DesktopUpdateService : UpdateServiceBase
{
    public DesktopUpdateService(HttpClient client, ILogger<DesktopUpdateService> logger) : base(client)
    {
        _logger = logger;
    }

    private readonly ILogger<DesktopUpdateService> _logger;

    public override async ValueTask CheckForUpdateAndDownloadInstall(bool isInBackground)
    {
        CheckForUpdateModel? updateModel;

        try
        {
            updateModel = await CheckForUpdate("exe", Assembly.GetExecutingAssembly().GetName().Version!);

            if (updateModel is null)
            {
                if (!isInBackground)
                    await SnackbarService.Default.ShowAsync("Update", "Failed to fetch update");

                return;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception when in {1}", nameof(CheckForUpdate));

            if (!isInBackground)
                await SnackbarService.Default.ShowAsync("Update", "Failed to fetch update");

            return;
        }

        if (!updateModel.NeedUpdate)
        {
            if (!isInBackground)
                await SnackbarService.Default.ShowAsync("Update", "You are using the latest version");

            return;
        }

        //TODO

        /*var control = _dialogService.GetDialogControl();
        control.ButtonLeftName = "Yes";
        control.ButtonRightName = "No";

        var buttonPressed = await control.ShowAndWaitAsync("Update",
            $"A new version - {updateModel.NewVersion} available" + "\n" + "Download and install now?");

        control.Hide();

        if (buttonPressed != IDialogControl.ButtonPressed.Left)
            return;

        try
        {
            await DownloadAndInstall(updateModel);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception when in {1}", nameof(DownloadAndInstall));
            await _snackbarService.ShowAsync("Update", "Failed to download and install new version");
        }*/
    }

    public override async Task DownloadAndInstall(CheckForUpdateModel updateModel)
    {
        const string newFileName = "new.exe";
        var newFilePath = Path.Combine(Directory.GetCurrentDirectory(), newFileName);

        await using (var stream = await Client.GetStreamAsync(updateModel.DownloadUrl))
        {
            await using var fileStream = new FileStream(newFilePath, FileMode.OpenOrCreate);
            await stream.CopyToAsync(fileStream);
        }

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
