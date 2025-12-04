using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IGoogleLensOcrDownloadService
{
    Task DownloadGoogleLensOcrStandalone(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken);

    Task DownloadGoogleLensOcrStandalone(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
}

public class GoogleLensOcrDownloadService(HttpClient httpClient) : IGoogleLensOcrDownloadService
{
    private const string WindowsUrl = "https://github.com/SubtitleEdit/support-files/releases/download/libmpv-2025-01-25/libmpv2-64.zip";

    public async Task DownloadGoogleLensOcrStandalone(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(httpClient, GetUrl(), destinationFileName, progress, cancellationToken);
    }
    
    public async Task DownloadGoogleLensOcrStandalone(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken)
    {
        await DownloadHelper.DownloadFileAsync(httpClient, GetUrl(), stream, progress, cancellationToken);
    }
    
    private string GetUrl()
    {
        return WindowsUrl;
    }
}