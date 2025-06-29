using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Video.AudioToTextWhisper.Engines;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Compression;
using Nikse.SubtitleEdit.Logic.Download;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Nikse.SubtitleEdit.Features.Video.AudioToTextWhisper;

public partial class DownloadWhisperEngineViewModel : ObservableObject
{
    [ObservableProperty] private string _titleText;
    [ObservableProperty] private double _progressOpacity;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private string _error;

    public Window? Window { get; set; }
    public bool OkPressed { get; internal set; }
    public IWhisperEngine? Engine { get; internal set; }

    private readonly IWhisperDownloadService _whisperDownloadService;
    private Task? _downloadTask;
    private readonly Timer _timer;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly MemoryStream _downloadStream;

    private readonly IZipUnpacker _zipUnpacker;

    public DownloadWhisperEngineViewModel(IWhisperDownloadService whisperDownloadService, IZipUnpacker zipUnpacker)
    {
        _whisperDownloadService = whisperDownloadService;
        _zipUnpacker = zipUnpacker;

        _cancellationTokenSource = new CancellationTokenSource();

        _downloadStream = new MemoryStream();

        TitleText = "Downloading Whisper engine";
        ProgressText = "Starting...";
        Error = string.Empty;

        _timer = new Timer(500);
        _timer.Elapsed += OnTimerOnElapsed;
        _timer.Start();
    }

    private readonly Lock _lockObj = new();

    private void OnTimerOnElapsed(object? sender, ElapsedEventArgs args)
    {
        lock (_lockObj)
        {
            _timer.Stop();
            if (_downloadTask is { IsCompleted: true } && Engine != null)
            {
                if (Engine.Name == WhisperEnginePurfviewFasterWhisperXxl.StaticName)
                {
                    var dir = Engine.GetAndCreateWhisperFolder();
                    var tempFileName = Path.Combine(dir, Engine.Name + ".7z");

                    ProgressText = "Unpacking 7-zip archive...";
                    Extract7Zip(tempFileName, dir);

                    try
                    {
                        File.Delete(tempFileName);
                    }
                    catch
                    {
                        // ignore
                    }

                    if (_cancellationTokenSource.IsCancellationRequested)
                    {
                        Cancel();
                        return;
                    }

                    OkPressed = true;
                    Close();
                }
                else
                {
                    if (_downloadStream.Length == 0)
                    {
                        ProgressText = "Download failed";
                        Error = "No data received";
                        return;
                    }

                    var folder = Engine.GetAndCreateWhisperFolder();
                    Unpack(folder, Engine.UnpackSkipFolder);
                    OkPressed = true;
                    Close();
                }

                return;
            }

            if (_downloadTask is { IsFaulted: true })
            {
                var ex = _downloadTask.Exception?.InnerException ?? _downloadTask.Exception;
                if (ex is OperationCanceledException)
                {
                    ProgressText = "Download canceled";
                    Close();
                }
                else
                {
                    ProgressText = "Download failed";
                    Error = ex?.Message ?? "Unknown error";
                }

                return;
            }

            _timer.Start();
        }
    }

    private void Extract7Zip(string tempFileName, string dir)
    {
        using Stream stream = File.OpenRead(tempFileName);
        using var archive = SevenZipArchive.Open(stream);
        double totalSize = archive.TotalUncompressSize;
        double unpackedSize = 0;

        var reader = archive.ExtractAllEntries();
        while (reader.MoveToNextEntry())
        {
            if (_cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            var skipFolderLevel = "Faster-Whisper-XXL";
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

                ProgressText = $"Unpacking: {displayName}";
                reader.WriteEntryToDirectory(fullPath, new ExtractionOptions() 
                { 
                    ExtractFullPath = false, 
                    Overwrite = true 
                });
                unpackedSize += reader.Entry.Size;
            }
        }

        ProgressValue = 100.0f;
    }

    private void Unpack(string folder, string skipFolderLevel)
    {
        _downloadStream.Position = 0;
        _zipUnpacker.UnpackZipStream(_downloadStream, folder, skipFolderLevel, false, new List<string>(), null);
        _downloadStream.Dispose();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var cppPath = Path.Combine(folder, "whisper-cli");
            if (File.Exists(cppPath))
            {
                MacHelper.MakeExecutable(folder);
            }
        }
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    [RelayCommand]
    private void CommandCancel()
    {
        Cancel();
    }

    private void Cancel()
    {
        _cancellationTokenSource?.Cancel();
        _timer.Stop();
        Close();
    }

    public void StartDownload()
    {
        TitleText = $"Downloading {Engine?.Name}";

        var downloadProgress = new Progress<float>(number =>
        {
            var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
            var pctString = percentage.ToString(CultureInfo.InvariantCulture);
            ProgressValue = percentage;
            ProgressText = $"Downloading... {pctString}%";
        });

        if (Engine is WhisperEngineCpp)
        {
            _downloadTask = _whisperDownloadService.DownloadWhisperCpp(_downloadStream, downloadProgress, _cancellationTokenSource.Token);
        }
        else if (Engine is WhisperEngineConstMe)
        {
            _downloadTask = _whisperDownloadService.DownloadWhisperConstMe(_downloadStream, downloadProgress, _cancellationTokenSource.Token);
        }
        else if (Engine is WhisperEnginePurfviewFasterWhisperXxl)
        {
            var dir = Engine.GetAndCreateWhisperFolder();
            var tempFileName = Path.Combine(dir, Engine.Name + ".7z");
            _downloadTask = _whisperDownloadService.DownloadWhisperPurfviewFasterWhisperXxl(tempFileName, downloadProgress, _cancellationTokenSource.Token);
        }
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        CommandCancel();
    }
}