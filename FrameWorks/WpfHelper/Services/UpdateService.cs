using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WpfHelper.Custom;

namespace WpfHelper.Services
{
    public static class UpdateService
    {
        #region Models

        private struct Asset
        {
            [JsonPropertyName("browser_download_url")]
            public string BrowserDownloadUrl { get; init; }

            [JsonPropertyName("name")]
            public string Name { get; init; }
        }
        
        private struct GitHubRequestApiModel
        {
            [JsonPropertyName("tag_name")]
            public string TagName { get; set; }

            [JsonPropertyName("assets")]
            public Asset[] Assets { get; set; }

            [JsonPropertyName("prerelease")]
            public bool Prerelease { get; set; }
        }

        public struct CheckForUpdateModel
        {
            public bool NeedUpdate { get; init; }

            public string AppFileName { get; init; }

            public string DownloadUrl { get; init; }

            public Version NewVersion { get; init; }
        }

        #endregion

        private static string? _gitHubUrl;

        public static string GitHubUrl
        {
            get
            {
                if (_gitHubUrl is null)
                    throw new ArgumentNullException(nameof(GitHubUrl));

                return _gitHubUrl;
            }
            set => _gitHubUrl = value;
        }

        /// <summary>
        /// Checks releases for new version
        /// </summary>
        /// <param name="appFileNameToDownload"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        public static async Task<CheckForUpdateModel> CheckForUpdate(string appFileNameToDownload)
        {
            using HttpResponseMessage response = await SimpleHttpRequestService.CreateResponse(GitHubUrl);

            await using Stream stream = await response.Content.ReadAsStreamAsync();
            GitHubRequestApiModel datApiModel = await JsonSerializer.DeserializeAsync<GitHubRequestApiModel>(stream);

            CheckTag(ref datApiModel);

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

            switch (newVersion.CompareTo(currentVersion))
            {
                case 0:
                case < 0:
                    return new CheckForUpdateModel()
                    {
                        AppFileName = appFileNameToDownload,
                        DownloadUrl = downloadUrl,
                        NewVersion = newVersion,
                        NeedUpdate = false
                    };
                case > 0:
                    return new CheckForUpdateModel()
                    {
                        AppFileName = appFileNameToDownload,
                        DownloadUrl = downloadUrl,
                        NewVersion = newVersion,
                        NeedUpdate = true
                    };
            }
        }

        /// <summary>
        /// Downloads and installs a new exe 
        /// </summary>
        /// <param name="model"></param>
        /// <returns>A new file name</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static async Task<string?> DownloadAndInstall(CheckForUpdateModel model)
        {
            string newFileName = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName!)!, $"{model.AppFileName.Remove(model.AppFileName.Length - 4, 4)}-{model.NewVersion}.exe");

            HttpResponseMessage response;
            Stream stream;
            try
            {
                response = await SimpleHttpRequestService.CreateResponse(model.DownloadUrl);
                stream = await response.Content.ReadAsStreamAsync();
            }
            catch
            {
                CustomMessageBox.Show("Failed to download update");
                return null;
            }

            await using (FileStream fileStream = new(newFileName, FileMode.Create))
                await stream.CopyToAsync(fileStream);

            await stream.DisposeAsync();
            response.Dispose();
            return newFileName;
        }

        /// <summary>
        /// To delete the previous application file after the update
        /// </summary>
        /// <param name="appFileName">without .exe</param>
        /// <returns></returns>
        public static async Task DeletePreviousFile(string appFileName)
        {
            await Task.Delay(5000);

            string[] files = await Task.Run(() => Directory.GetFiles(Environment.CurrentDirectory));
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

        #region PrivateMethods

        private static void CheckTag(ref GitHubRequestApiModel model)
        {
            if (model.TagName.Contains("v") || model.TagName.Contains("V"))
                model.TagName = model.TagName.Remove(0, 1);

            if (model.TagName.Contains("RC"))
                model.TagName = model.TagName.Remove(model.TagName.LastIndexOf("R", StringComparison.Ordinal) - 1, 3);
        }

        #endregion
    }
}
