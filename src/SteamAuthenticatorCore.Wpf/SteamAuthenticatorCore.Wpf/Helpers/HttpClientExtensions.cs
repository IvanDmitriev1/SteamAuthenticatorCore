﻿using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace SteamAuthenticatorCore.Desktop.Helpers;

public static class HttpClientExtensions
{
    public static async Task DownloadAsync(this HttpClient client, string requestUri, Stream destination, IProgress<double> progress, CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (response.Content.Headers.ContentLength is null)
            return;

        var totalDownloadSize = response.Content.Headers.ContentLength;

        await using var download = await response.Content.ReadAsStreamAsync(cancellationToken);

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