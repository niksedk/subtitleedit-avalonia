using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Logic.Download;

public interface ISpellCheckDictionaryDownloadService
{
    Task DownloadDictionary(Stream stream, string url, IProgress<float>? progress, CancellationToken cancellationToken);
}