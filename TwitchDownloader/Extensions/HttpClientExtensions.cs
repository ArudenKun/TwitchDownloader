using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TwitchDownloader.Extensions;

public static class HttpClientExtensions
{
    public static async Task DownloadAsync(
        this HttpClient client,
        string requestUri,
        Stream destination,
        IProgress<float>? progress = null,
        CancellationToken cancellationToken = default
    ) => await client.DownloadAsync(new Uri(requestUri), destination, progress, cancellationToken);

    public static async Task DownloadAsync(
        this HttpClient client,
        Uri requestUri,
        Stream destination,
        IProgress<float>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        // Get the http headers first to examine the content length
        using var response = await client.GetAsync(
            requestUri,
            HttpCompletionOption.ResponseHeadersRead,
            CancellationToken.None
        );
        if (!response.IsSuccessStatusCode)
            throw new IOException($"Failed to download {requestUri}");

        var contentLength = response.Content.Headers.ContentLength;

        await using var download = await response.Content.ReadAsStreamAsync(CancellationToken.None);
        // Ignore progress reporting when no progress reporter was
        // passed or when the content length is unknown
        if (progress == null || !contentLength.HasValue)
        {
            await download.CopyToAsync(destination, cancellationToken);
            return;
        }

        // Convert absolute progress (bytes downloaded) into relative progress (0% - 100%)
        var relativeProgress = new Progress<long>(totalBytes =>
            progress.Report((float)totalBytes / contentLength.Value)
        );
        // Use extension method to report progress while downloading
        await download.CopyToAsync(destination, 81920, relativeProgress, cancellationToken);
        progress.Report(1);
    }

    public static async Task DownloadAsync(
        this HttpClient client,
        string requestUri,
        string destinationPath,
        bool overwrite = true,
        IProgress<float>? progress = null,
        CancellationToken cancellationToken = default
    ) =>
        await client.DownloadAsync(
            new Uri(requestUri),
            destinationPath,
            overwrite,
            progress,
            cancellationToken
        );

    public static async Task DownloadAsync(
        this HttpClient client,
        Uri requestUri,
        string destinationPath,
        bool overwrite = true,
        IProgress<float>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        await using var destinationStream = new FileStream(
            destinationPath,
            overwrite ? FileMode.Create : FileMode.CreateNew
        );
        await client.DownloadAsync(requestUri, destinationStream, progress, cancellationToken);
    }
}
