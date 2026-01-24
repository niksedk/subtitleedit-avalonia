using Avalonia.Threading;
using Nikse.SubtitleEdit.Logic.Config;
using SevenZipExtractor;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.IO;
using System.Threading;

namespace Nikse.SubtitleEdit.Logic.SevenZipExtractor;

public static class Unpacker
{
    public static void Extract7Zip(string tempFileName, string dir, string skipFolderLevel, CancellationTokenSource cancellationTokenSource, Action<string> updateProgressText)
    {
        Dispatcher.UIThread.Post(() =>
        {
            updateProgressText(Se.Language.General.Unpacking7ZipArchiveDotDotDot);
        });

        if (!OperatingSystem.IsWindows())
        {
            Extract7ZipSlow(tempFileName, dir, skipFolderLevel, cancellationTokenSource, updateProgressText);
            return;
        }

        Unpack7ZipViaNativeLibrary(tempFileName, dir, skipFolderLevel, cancellationTokenSource, updateProgressText);
    }

    private static void Unpack7ZipViaNativeLibrary(string tempFileName, string dir, string skipFolderLevel, CancellationTokenSource cancellationTokenSource, Action<string> updateProgressText)
    {
        using (var archiveFile = new ArchiveFile(tempFileName))
        {
            archiveFile.Extract(entry =>
            {
                if (cancellationTokenSource.IsCancellationRequested)
                {
                    return null;
                }

                var entryFullName = entry.FileName;
                if (!string.IsNullOrEmpty(skipFolderLevel) && entryFullName.StartsWith(skipFolderLevel))
                {
                    entryFullName = entryFullName.Substring(skipFolderLevel.Length);
                }

                entryFullName = entryFullName.Replace('/', Path.DirectorySeparatorChar);
                entryFullName = entryFullName.TrimStart(Path.DirectorySeparatorChar);

                var fullFileName = Path.Combine(dir, entryFullName);

                var fullPath = Path.GetDirectoryName(fullFileName);
                if (fullPath == null)
                {
                    return null;
                }


                var displayName = entryFullName;
                if (displayName.Length > 30)
                {
                    displayName = "..." + displayName.Remove(0, displayName.Length - 26).Trim();
                }

                Dispatcher.UIThread.Post(() =>
                {
                    updateProgressText(string.Format(Se.Language.General.UnpackingX, displayName));
                });

                return fullFileName;
            });
        }
    }

    public static void Extract7ZipSlow(string tempFileName, string dir, string skipFolderLevel, CancellationTokenSource cancellationTokenSource, Action<string> updateProgressText)
    {
        using Stream stream = File.OpenRead(tempFileName);
        using var archive = SevenZipArchive.Open(stream);
        double totalSize = archive.TotalUncompressSize;
        double unpackedSize = 0;

        var reader = archive.ExtractAllEntries();
        while (reader.MoveToNextEntry())
        {
            if (cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            if (!string.IsNullOrEmpty(reader.Entry.Key))
            {
                var entryFullName = reader.Entry.Key;
                if (!string.IsNullOrEmpty(skipFolderLevel) && entryFullName.StartsWith(skipFolderLevel))
                {
                    entryFullName = entryFullName[skipFolderLevel.Length..];
                }

                entryFullName = entryFullName.Replace('/', Path.DirectorySeparatorChar);
                entryFullName = entryFullName.TrimStart(Path.DirectorySeparatorChar);

                var fullFileName = Path.Combine(dir, entryFullName);

                if (reader.Entry.IsDirectory)
                {
                    if (!Directory.Exists(fullFileName))
                    {
                        Directory.CreateDirectory(fullFileName);
                    }

                    continue;
                }

                var fullPath = Path.GetDirectoryName(fullFileName);
                if (fullPath == null)
                {
                    continue;
                }

                var displayName = entryFullName;
                if (displayName.Length > 30)
                {
                    displayName = "..." + displayName.Remove(0, displayName.Length - 26).Trim();
                }
                Dispatcher.UIThread.Post(() =>
                {
                    updateProgressText(string.Format(Se.Language.General.UnpackingX, displayName));
                });

                reader.WriteEntryToDirectory(fullPath, new ExtractionOptions()
                {
                    ExtractFullPath = false,
                    Overwrite = true
                });
                unpackedSize += reader.Entry.Size;
            }
        }
    }
}
