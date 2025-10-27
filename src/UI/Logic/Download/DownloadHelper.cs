using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public static class DownloadHelper
{
    public static async Task DownloadFileAsync(
        HttpClient httpClient, 
        string url, 
        string destinationPath, 
        IProgress<float>? progress, 
        CancellationToken cancellationToken)
    {
        await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
        await DownloadFileAsync(httpClient, url, fileStream, progress, cancellationToken);
    }

    public static async Task DownloadFileAsync(
      HttpClient httpClient,
      string url,
      Stream destination,
      IProgress<float>? progress,
      CancellationToken cancellationToken,
      int maxRetries = 3,
      int timeoutSeconds = 30)
    {
        if (httpClient == null)
        {
            throw new ArgumentNullException(nameof(httpClient));
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("URL cannot be null or empty", nameof(url));
        }

        if (destination == null)
        {
            throw new ArgumentNullException(nameof(destination));
        }

        if (!destination.CanWrite)
        {
            throw new ArgumentException("Destination stream must be writable", nameof(destination));
        }

        var attempt = 0;
        Exception? lastException = null;

        while (attempt < maxRetries)
        {
            attempt++;

            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

                using var response = await httpClient.GetAsync(
                    url,
                    HttpCompletionOption.ResponseHeadersRead,
                    cts.Token).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                var totalReadBytes = 0L;
                var startPosition = destination.Position;

                await using var contentStream = await response.Content.ReadAsStreamAsync(cts.Token).ConfigureAwait(false);

                var buffer = new byte[81920]; // 80KB buffer for better performance
                int readBytes;

                while ((readBytes = await contentStream.ReadAsync(buffer, cts.Token).ConfigureAwait(false)) > 0)
                {
                    await destination.WriteAsync(buffer.AsMemory(0, readBytes), cts.Token).ConfigureAwait(false);
                    totalReadBytes += readBytes;

                    if (progress != null && totalBytes > 0)
                    {
                        var progressPercentage = (float)totalReadBytes / totalBytes;
                        progress.Report(Math.Clamp(progressPercentage, 0f, 1f));
                    }
                }

                // Verify download completeness if Content-Length was provided
                if (totalBytes > 0 && totalReadBytes != totalBytes)
                {
                    throw new InvalidOperationException(
                        $"Download incomplete: expected {totalBytes} bytes, received {totalReadBytes} bytes");
                }

                await destination.FlushAsync(cts.Token).ConfigureAwait(false);

                // Success - report 100% if we haven't already
                progress?.Report(1f);
                return;
            }
            catch (Exception ex) when (
                ex is HttpRequestException ||
                ex is TaskCanceledException ||
                ex is IOException ||
                ex is InvalidOperationException)
            {
                lastException = ex;

                // If this was the last retry or cancellation was requested, don't retry
                if (attempt >= maxRetries || cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                // Exponential backoff: wait before retrying
                var delayMs = Math.Min(1000 * (int)Math.Pow(2, attempt - 1), 10000);
                await Task.Delay(delayMs, CancellationToken.None).ConfigureAwait(false);

                // Reset stream position if possible for retry
                if (destination.CanSeek)
                {
                    destination.Position = 0;
                }
            }
        }

        // All retries exhausted
        throw new InvalidOperationException(
            $"Failed to download file after {maxRetries} attempts. URL: {url}",
            lastException);
    }
}