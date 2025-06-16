using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IFfmpegDownloadService
{
    Task DownloadFfmpeg(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadFfmpeg(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
}