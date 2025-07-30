using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IPaddleOcrDownloadService
{
    Task DownloadModels(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadEngineCpu(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken);
    Task DownloadEngineGpu(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken);
}