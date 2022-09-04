using SteamAuthenticatorCore.Shared.Abstractions;
using SteamAuthenticatorCore.Shared.Models;

namespace SteamAuthenticatorCore.MobileMaui.Services;

public class UpdateService : IUpdateService
{
    public ValueTask CheckForUpdateAndDownloadInstall(bool isInBackground) => throw new NotImplementedException();

    public ValueTask<CheckForUpdateModel?> CheckForUpdate(string fileContains, Version currentVersion) => throw new NotImplementedException();

    public Task DownloadAndInstall(CheckForUpdateModel updateModel) => throw new NotImplementedException();
}
