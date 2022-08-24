using System;
using System.Threading.Tasks;
using SteamAuthenticatorCore.Shared.Models;

namespace SteamAuthenticatorCore.Shared.Abstractions;

public interface IUpdateService
{
    ValueTask CheckForUpdateAndDownloadInstall(bool isInBackground);
    ValueTask<CheckForUpdateModel?> CheckForUpdate(string fileName, Version currentVersion);
    Task DownloadAndInstall(CheckForUpdateModel updateModel);
}
