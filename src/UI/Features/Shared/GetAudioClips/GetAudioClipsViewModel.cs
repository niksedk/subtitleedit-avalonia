using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Video.ShotChanges;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Nikse.SubtitleEdit.Features.Shared.GetAudioClips;

public partial class GetAudioClipsViewModel : ObservableObject
{
    [ObservableProperty] private double _progress;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private string _error;

    public Window? Window { get; set; }
    public List<string> AudioClips { get; set; }

    private Task? _downloadTask;
    private readonly Timer _timer;
    private bool _done;
    private string _videoFileName;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly MemoryStream _downloadStream;
    private Process? _ffmpegProcess;
    private List<SubtitleLineViewModel> _lines;

    public GetAudioClipsViewModel()
    {
        _cancellationTokenSource = new CancellationTokenSource();

        _downloadStream = new MemoryStream();

        StatusText = Se.Language.General.StartingDotDotDot;
        Error = string.Empty;
        AudioClips = new List<string>();
        _videoFileName = string.Empty;
        _lines = new List<SubtitleLineViewModel>();

        _timer = new Timer(500);
        _timer.Elapsed += OnTimerOnElapsed;
        _timer.Start();
    }

    public void Initialize(string videoFileName, List<SubtitleLineViewModel> lines)
    {
        _videoFileName = videoFileName;
        _lines = lines;
    }

    private void ExtractLines()
    {
        foreach (var line in _lines)
        {
            var outputFileName = Path.Combine(Path.GetTempPath(), $"subtitleedit_audioclip_{Guid.NewGuid()}.wav");
            var process = GetExtractProcess(_videoFileName, line, outputFileName);
            _ffmpegProcess = process;
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
            if (process.ExitCode == 0 && File.Exists(outputFileName))
            {
                AudioClips.Add(outputFileName);
            }
            else
            {
                //Error = Se.Language.Video.CouldNotExtractAudioClip;
            }
        }
    }

    private Process GetExtractProcess(string videoFileName, SubtitleLineViewModel line, string outputFileName)
    {
        var useCenterChannelOnly = false; // Se.Settings.Waveform.cen;

        var arguments = FfmpegGenerator.ExtractAudioClipFromVideoParameters(
            videoFileName,
            line.StartTime.TotalSeconds,
            line.EndTime.TotalSeconds - line.StartTime.TotalSeconds,
            useCenterChannelOnly,
            outputFileName);

        return FfmpegGenerator.GetProcess(arguments, OutputHandler);
    }

    private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (string.IsNullOrWhiteSpace(outLine.Data))
        {
            return;
        }

        var match = ShotChangesViewModel.TimeRegex.Match(outLine.Data);
        if (match.Success)
        {
            var timeCode = match.Value.Replace("pts_time:", string.Empty).Replace(",", ".").Replace("٫", ".").Replace("⠨", ".");
            if (double.TryParse(timeCode, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var seconds) && seconds > 0.2)
            {
                //lock (_addShotChangeLock)
                //{
                //    AudioVisualizer?.ShotChanges.Add(seconds);
                //}
            }
        }
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

                Close();
            }
            else if (_downloadTask is { IsFaulted: true })
            {
                _timer.Stop();
                _done = true;
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
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            CommandCancel();
        }
    }
}