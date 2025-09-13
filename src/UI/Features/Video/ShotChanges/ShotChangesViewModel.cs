using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Video.ShotChanges;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;

namespace Nikse.SubtitleEdit.Features.Tools.ApplyDurationLimits;

public partial class ShotChangesViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<TimeItem> _ffmpegLines;
    [ObservableProperty] private TimeItem? _selectedFfmpegLine;
    [ObservableProperty] private string? _importText;
    [ObservableProperty] private double _sensitivity;
    [ObservableProperty] private bool _timeCodeFrames;
    [ObservableProperty] private bool _timeCodeSeconds;
    [ObservableProperty] private bool _timeCodeMilliseconds;
    [ObservableProperty] private bool _timeCodeHhMmSsMs;
    [ObservableProperty] private double _progressValue;
    [ObservableProperty] private bool _isGenerating;

    public Window? Window { get; set; }
    public double LastSeconds { get; private set; }
    public bool OkPressed { get; private set; }

    private StringBuilder Log { get; set; } = new StringBuilder();
    private string _videoFileName = string.Empty;
    private List<double> _shotChanges = new();
    private Process? _ffmpegProcess;
    private readonly System.Timers.Timer _timerGenerate;
    private bool _doAbort;
    private FfmpegMediaInfo? _mediaInfo;
    private static readonly Regex TimeRegex = new Regex(@"pts_time:\d+[.,]*\d*", RegexOptions.Compiled);
    private Lock TimeCodesLock = new Lock();

    public ShotChangesViewModel()
    {
        FfmpegLines = new ObservableCollection<TimeItem>();
        LoadSettings();

        _timerGenerate = new();
        _timerGenerate.Elapsed += TimerGenerateElapsed;
        _timerGenerate.Interval = 100;
    }

    private void LoadSettings()
    {
        Sensitivity = Se.Settings.Waveform.ShotChangesSensitivity;
    }

    private void SaveSettings()
    {
        Se.Settings.Waveform.ShotChangesSensitivity = Sensitivity;
        Se.SaveSettings();
    }

    private void TimerGenerateElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_ffmpegProcess == null)
        {
            return;
        }

        if (_doAbort)
        {
            _timerGenerate.Stop();
#pragma warning disable CA1416
            _ffmpegProcess.Kill(true);
#pragma warning restore CA1416

            IsGenerating = false;
            return;
        }

        if (!_ffmpegProcess.HasExited)
        {
            return;
        }

        _timerGenerate.Stop();

    }

    [RelayCommand]
    private void GenerateShotChangesFfmpeg()
    {
        IsGenerating = true;

        var threshold = Sensitivity.ToString(CultureInfo.InvariantCulture);
        var argumentsFormat = "-i \"{0}\" -vf \"select=gt(scene\\,{1}),showinfo\" -threads 0 -vsync vfr -f null -";
        var arguments = string.Format(argumentsFormat, _videoFileName, threshold);

        ProgressValue = 0;
        _ffmpegProcess = FfmpegGenerator.GetProcess(arguments, OutputHandler);
#pragma warning disable CA1416 // Validate platform compatibility
        _ffmpegProcess.Start();
#pragma warning restore CA1416 // Validate platform compatibility
        _ffmpegProcess.BeginOutputReadLine();
        _ffmpegProcess.BeginErrorReadLine();
    }

    private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (string.IsNullOrWhiteSpace(outLine.Data))
        {
            return;
        }

        Log.AppendLine(outLine.Data);
        var match = TimeRegex.Match(outLine.Data);
        if (match.Success)
        {
            var timeCode = match.Value.Replace("pts_time:", string.Empty).Replace(",", ".").Replace("٫", ".").Replace("⠨", ".");
            if (double.TryParse(timeCode, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var seconds) && seconds > 0.2)
            {
                lock (TimeCodesLock)
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        FfmpegLines.Add(new TimeItem(seconds, FfmpegLines.Count));
                    });
                }
                LastSeconds = seconds;
            }
        }
    }

    [RelayCommand]
    private void ImportFromTextFile()
    {
    }

    [RelayCommand]
    private void Ok()
    {
        SaveSettings();
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

    internal void Initialize(string videoFileName, List<double> shotChanges)
    {
        _videoFileName = videoFileName;
        _shotChanges = shotChanges;
    }
}