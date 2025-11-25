using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface IYtDlpDownloadService
{
    Task DownloadYtDlp(IProgress<float>? progress, CancellationToken cancellationToken);
}