using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface ILibMpvDownloadService
{
    Task DownloadLibMpv(string destinationFileName, IProgress<float>? progress, CancellationToken cancellationToken);

    Task DownloadLibMpv(Stream stream, IProgress<float>? progress, CancellationToken cancellationToken);
}