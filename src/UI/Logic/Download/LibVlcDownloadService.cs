using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface ILibVlcDownloadService
{
    Task DownloadLibVlc(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken);

    Task DownloadLibVlc(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
}

public class LibVlcDownloadService(HttpClient httpClient) : ILibVlcDownloadService
{
    private const string WindowsUrl = "https://get.videolan.org/vlc/3.0.21/win64/vlc-3.0.21-win64.7z";

    public async Task DownloadLibVlc(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(httpClient, GetUrl(), destinationFileName, progress, cancellationToken);
    }

    public async Task DownloadLibVlc(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(httpClient, GetUrl(), stream, progress, cancellationToken);
    }

    private string GetUrl()
    {
        if (OperatingSystem.IsWindows())
        {
            return WindowsUrl;
        }

        throw new PlatformNotSupportedException("LibVLC download is only supported on Windows.");
    }
}