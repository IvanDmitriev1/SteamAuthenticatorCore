using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Sentry;
using SteamAuthenticatorCore.Shared.Models;
using SteamAuthenticatorCore.Shared.Services;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

namespace SteamAuthenticatorCore.Desktop.Services;

internal class DesktopUpdateService : UpdateServiceBase
{
    public DesktopUpdateService(HttpClient client, ISnackbarService snackbarService, IDialogService dialogService, IHub hub) : base(client)
    {
        _snackbarService = snackbarService;
        _dialogService = dialogService;
        _hub = hub;
    }

    private readonly ISnackbarService _snackbarService;
    private readonly IDialogService _dialogService;
    private readonly IHub _hub;

    public async override ValueTask CheckForUpdateAndDownloadInstall(bool isInBackground)
    {
        CheckForUpdateModel? updateModel = null!;

        try
        {
            updateModel = await CheckForUpdate($"{Assembly.GetExecutingAssembly().GetName().Name}.exe");

            if (updateModel is null)
            {
                if (!isInBackground)
                    await _snackbarService.ShowAsync("Update", "Failed to check update");

                return;
            }
        }
        catch (Exception e)
        {
            _hub.CaptureException(e);

            if (!isInBackground)
                await _snackbarService.ShowAsync("Update", "Failed to check update");
            
            return;
        }

        if (!updateModel.NeedUpdate)
        {
            if (!isInBackground)
                await _snackbarService.ShowAsync("Update", "You are using the latest version");

            return;
        }

        var control = _dialogService.GetDialogControl();
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
            _hub.CaptureException(e);
            await _snackbarService.ShowAsync("Update", "Failed to download and install new version");
        }
    }

    public async override Task DownloadAndInstall(CheckForUpdateModel updateModel)
    {
        var fileName = $"{Assembly.GetExecutingAssembly().GetName().Name}-{updateModel.NewVersion}.exe";
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

        await using var stream = await Client.GetStreamAsync(updateModel.DownloadUrl);

        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await stream.CopyToAsync(fileStream);
    }
}
