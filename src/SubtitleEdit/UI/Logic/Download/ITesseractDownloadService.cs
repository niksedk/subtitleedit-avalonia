using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface ITesseractDownloadService
{
    Task DownloadTesseract(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadTesseractModel(string modelUrl, Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
}