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
        IProgress<float>? progress ,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1L;
        var totalReadBytes = 0L;

        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);

        var buffer = new byte[8192];
        var isMoreToRead = true;

        do
        {
            var readBytes = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
            if (readBytes == 0)
            {
                isMoreToRead = false;
            }
            else
            {
                await destination.WriteAsync(buffer, 0, readBytes, cancellationToken);

                totalReadBytes += readBytes;
                if (progress != null && totalBytes > 0)
                {
                    var progressPercentage = (float)totalReadBytes / totalBytes;
                    progress.Report(progressPercentage);
                }
            }
        }
        while (isMoreToRead);
    }
}