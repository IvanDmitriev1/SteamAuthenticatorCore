using System.Net.Http;

namespace SteamAuthenticatorCore.Shared.Extensions;

public static class HttpClientExtensions
{
    public static async Task DownloadAsync(this HttpResponseMessage message, Stream destination, IProgress<double> progress, CancellationToken cancellationToken)
    {
        if (message.Content.Headers.ContentLength is null)
            return;

        var totalDownloadSize = message.Content.Headers.ContentLength;

        await using var download = await message.Content.ReadAsStreamAsync(cancellationToken);

        var buffer = new byte[81920];
        long totalBytesRead = 0;
        int bytesRead;

        while ((bytesRead = await download.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) != 0)
        {
            await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);

            totalBytesRead += bytesRead;
            var progressPercentage = Math.Round(totalBytesRead / (double) totalDownloadSize * 100, 2);

            progress.Report(progressPercentage);
        }
    }
}