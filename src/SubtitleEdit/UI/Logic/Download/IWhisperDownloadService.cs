using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IWhisperDownloadService
{
    Task DownloadFile(string url, string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadWhisperCpp(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadWhisperConstMe(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadWhisperPurfviewFasterWhisperXxl(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken);
}