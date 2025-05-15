using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IPaddleOcrDownloadService
{
    Task DownloadModels(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
}