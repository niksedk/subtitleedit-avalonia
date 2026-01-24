using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using SevenZipExtractor;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;
using SharpCompress.Readers;
using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Nikse.SubtitleEdit.Features.Ocr.Download;

public partial class DownloadPaddleOcrViewModel : ObservableObject
{
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private string _progressText;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private string _error;

    public Window? Window { get; set; }
    public bool OkPressed { get; internal set; }

    private string _tempFileName;
    private IPaddleOcrDownloadService _paddleOcrDownloadService;
    private Task? _downloadTask;
    private  Timer _timer = new Timer(500);
    private bool _done;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private PaddleOcrDownloadType _downloadType;

    public DownloadPaddleOcrViewModel(IPaddleOcrDownloadService paddleOcrDownloadService)
    {
        _paddleOcrDownloadService = paddleOcrDownloadService;

        _cancellationTokenSource = new CancellationTokenSource();

        StatusText = Se.Language.General.StartingDotDotDot;
        ProgressText = string.Empty;
        Error = string.Empty;
        _tempFileName = string.Empty;
        _downloadType = PaddleOcrDownloadType.Models;
    }

    private readonly Lock _lockObj = new();

    public void Initialize(PaddleOcrDownloadType paddleOcrDownloadType)
    {
        _downloadType = paddleOcrDownloadType;
        if (_downloadType == PaddleOcrDownloadType.EngineCpu)
        {
            StatusText = "Downloading Paddle OCR engine...";
        }
        else if (_downloadType == PaddleOcrDownloadType.EngineGpu)
        {
            StatusText = "Downloading Paddle OCR engine...";
        }
        else if (_downloadType == PaddleOcrDownloadType.Models)
        {
            StatusText = "Downloading Paddle OCR models...";
        }
    }

    private void OnTimerOnElapsed(object? sender, ElapsedEventArgs args)
    {
        lock (_lockObj)
        {
            if (_done)
            {
                return;
            }

            if (_downloadTask is { IsCompleted: true }) 
            {
                _timer.Stop();
                _done = true;

                if (!File.Exists(_tempFileName))
                {
                    ProgressText = "Download failed";
                    Error = "No data received";
                    return;
                }

                var fileInfo = new FileInfo(_tempFileName);
                if (fileInfo.Length == 0)
                {
                    ProgressText = "Download failed";
                    Error = "No data received";
                    return;
                }

                if (_downloadType == PaddleOcrDownloadType.Models)
                {
                    StatusText = "Unpacking Paddle OCR models...";
                    Extract7Zip(_tempFileName, Se.PaddleOcrModelsFolder, "PaddleOCR.PP-OCRv5.support.files");
                }
                else if (_downloadType == PaddleOcrDownloadType.EngineGpu)
                {
                    StatusText = "Unpacking Paddle OCR GPU...";
                    Extract7Zip(_tempFileName, Se.PaddleOcrFolder, "PaddleOCR-GPU-v1.3.2-CUDA-11.8");
                }
                else if (_downloadType == PaddleOcrDownloadType.EngineCpu)
                {
                    StatusText = "Unpacking Paddle OCR CPU...";
                    Extract7Zip(_tempFileName, Se.PaddleOcrFolder, "PaddleOCR-CPU-v1.3.2");
                }

                OkPressed = true;
                Close();
            }
            else if (_downloadTask is { IsFaulted: true })
            {
                _timer.Stop();
                _done = true;
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
            }
        }
    }

    private void Extract7Zip(string tempFileName, string dir, string skipFolderLevel)
    {
        StatusText = Se.Language.General.Unpacking7ZipArchiveDotDotDot;
        if (!OperatingSystem.IsWindows())
        {
            Extract7ZipSlow(tempFileName, dir, skipFolderLevel);
            return; 
        }

        using (var archiveFile = new ArchiveFile(tempFileName))
        {
            archiveFile.Extract(entry =>
            {
                if (_cancellationTokenSource.IsCancellationRequested)
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
                    ProgressText = string.Format(Se.Language.General.UnpackingX, displayName);
                });

                return fullFileName;
            });
        }

        ProgressValue = 100.0f;
    }

    private void Extract7ZipSlow(string tempFileName, string dir, string skipFolderLevel)
    {
        StatusText = Se.Language.General.Unpacking7ZipArchiveDotDotDot;
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
                    ProgressText = string.Format(Se.Language.General.UnpackingX, displayName);
                });

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
        _cancellationTokenSource?.Cancel();
        _done = true;
        _timer.Stop();
        Close();
    }

    public void StartDownload()
    {
        var downloadProgress = new Progress<float>(number =>
        {
            var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
            var pctString = percentage.ToString(CultureInfo.InvariantCulture);
            ProgressValue = percentage;
            ProgressText = string.Format(Se.Language.General.DownloadingXPercent, pctString);
        });

        var folder = Se.PaddleOcrFolder;
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        _tempFileName = Path.Combine(folder, $"{Guid.NewGuid()}.7z");
        //_tempFileName = "C:\\temp\\PaddleOCR-GPU-v1.3.2-CUDA-11.8.7z"; //TODO: remove;
        //_timer.Elapsed += OnTimerOnElapsed;
        //_timer.Start();
        //return;

        if (_downloadType == PaddleOcrDownloadType.Models)
        {
            _downloadTask = _paddleOcrDownloadService.DownloadModels(_tempFileName, downloadProgress, _cancellationTokenSource.Token);
        }
        else if (_downloadType == PaddleOcrDownloadType.EngineGpu)
        {
            _downloadTask = _paddleOcrDownloadService.DownloadEngineGpu(_tempFileName, downloadProgress, _cancellationTokenSource.Token);
        }
        else
        {
            _downloadTask = _paddleOcrDownloadService.DownloadEngineCpu(_tempFileName, downloadProgress, _cancellationTokenSource.Token);
        }

        _timer.Elapsed += OnTimerOnElapsed;
        _timer.Start();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            CommandCancel();
        }
    }
}