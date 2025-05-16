using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Logic.Compression;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Download;
using Timer = System.Timers.Timer;

namespace Nikse.SubtitleEdit.Features.Common;

public partial class DownloadFfmpegViewModel : ObservableObject
{
    [ObservableProperty] private double _progress;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private string _error;

    public DownloadFfmpegWindow? Window { get; set; }
    public string FfmpegFileName { get; set; }

    private IFfmpegDownloadService _ffmpegDownloadService;
    private Task? _downloadTask;
    private readonly Timer _timer;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly MemoryStream _downloadStream;

    private readonly IZipUnpacker _zipUnpacker;

    public DownloadFfmpegViewModel(IFfmpegDownloadService ffmpegDownloadService, IZipUnpacker zipUnpacker)
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

    private void OnTimerOnElapsed(object? sender, ElapsedEventArgs args)
    {
        if (_downloadTask is { IsCompleted: true })
        {
            _timer.Stop();

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

            FfmpegFileName = ffmpegFileName;
            Close();
        }
        else if (_downloadTask is { IsFaulted: true })
        {
            _timer.Stop();
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
        Window?.Close();
    }

    [RelayCommand]
    private void CommandCancel()
    {
        _cancellationTokenSource?.Cancel();
        Window?.Close();
    }

    public void StartDownload()
    {
        var downloadProgress = new Progress<float>(number =>
        {
            var percentage = (int)Math.Round(number * 100.0, MidpointRounding.AwayFromZero);
            var pctString = percentage.ToString(CultureInfo.InvariantCulture);
            Progress = number;
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
}