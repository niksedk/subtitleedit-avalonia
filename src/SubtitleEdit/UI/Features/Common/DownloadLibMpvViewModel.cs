using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Compression;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Timer = System.Timers.Timer;

namespace Nikse.SubtitleEdit.Features.Common;

public partial class DownloadLibMpvViewModel : ObservableObject
{
    [ObservableProperty] private double _progress;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private string _error;

    public DownloadLibMpvWindow? Window { get; set; }
    public string FfmpegFileName { get; set; }

    private IFfmpegDownloadService _ffmpegDownloadService;
    private Task? _downloadTask;
    private readonly Timer _timer;
    private bool _done;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly MemoryStream _downloadStream;

    private readonly IZipUnpacker _zipUnpacker;

    public DownloadLibMpvViewModel(IFfmpegDownloadService ffmpegDownloadService, IZipUnpacker zipUnpacker)
    {
        _ffmpegDownloadService = ffmpegDownloadService;
        _zipUnpacker = zipUnpacker;

        _cancellationTokenSource = new CancellationTokenSource();

        _downloadStream = new MemoryStream();

        StatusText = "Starting...";
        Error = string.Empty;
        FfmpegFileName = string.Empty;

        _timer = new Timer(500);
        _timer.Elapsed += OnTimerOnElapsed;
        _timer.Start();
    }

    private readonly Lock _lockObj = new();

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

                if (_downloadStream.Length == 0)
                {
                    StatusText = "Download failed";
                    Error = "No data received";
                    return;
                }

                var ffmpegFileName = GetFfmpegFileName();

                if (File.Exists(ffmpegFileName))
                {
                    File.Delete(ffmpegFileName);
                }

                UnpackFfmpeg(ffmpegFileName);
                
                if (File.Exists(ffmpegFileName) && RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    MacHelper.MakeExecutable(ffmpegFileName);   
                }
                
                FfmpegFileName = ffmpegFileName;
                Close();
            }
            else if (_downloadTask is { IsFaulted: true })
            {
                _timer.Stop();
                _done = true;
                var ex = _downloadTask.Exception?.InnerException ?? _downloadTask.Exception;
                if (ex is OperationCanceledException)
                {
                    StatusText = "Download canceled";
                    Close();
                }
                else
                {
                    StatusText = "Download failed";
                    Error = ex?.Message ?? "Unknown error";
                }
            }
        }
    }

    private void UnpackFfmpeg(string newFileName)
    {
        var folder = Path.GetDirectoryName(newFileName);
        if (folder != null)
        {
            _downloadStream.Position = 0;
            _zipUnpacker.UnpackZipStream(_downloadStream, folder);
        }

        _downloadStream.Dispose();
    }


    public static string GetFfmpegFileName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Path.Combine(Se.FfmpegFolder, "ffmpeg.exe");
        }

        return Path.Combine(Se.FfmpegFolder, "ffmpeg");
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
            Progress = percentage;
            StatusText = $"Downloading... {pctString}%";
        });

        var folder = Se.FfmpegFolder;
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        _downloadTask = _ffmpegDownloadService.DownloadFfmpeg(
            _downloadStream,
            downloadProgress,
            _cancellationTokenSource.Token);
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        CommandCancel();
    }
}