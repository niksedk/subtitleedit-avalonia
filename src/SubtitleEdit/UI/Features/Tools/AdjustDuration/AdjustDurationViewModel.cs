using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;

namespace Nikse.SubtitleEdit.Features.Tools.AdjustDuration;

public partial class AdjustDurationViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<AdjustDurationDisplay> _adjustTypes;
    [ObservableProperty] private AdjustDurationDisplay _selectedAdjustType;

    [ObservableProperty] private double _adjustSeconds;
    [ObservableProperty] private int _adjustPercent;
    [ObservableProperty] private double _adjustFixed;
    [ObservableProperty] private double _adjustRecalculateMaxCharacterPerSecond;
    [ObservableProperty] private double _adjustRecalculateOptimalCharacterPerSecond;

    public AdjustDurationWindow? Window { get; set; }

    public bool OkPressed { get; private set; }

    public AdjustDurationViewModel()
    {
        AdjustTypes = new ObservableCollection<AdjustDurationDisplay>(AdjustDurationDisplay.ListAll());
        SelectedAdjustType = AdjustTypes[0];
        LoadSettings();
    }

    public void Adjust(ObservableCollection<SubtitleLineViewModel> subtitles)
    {
        if (SelectedAdjustType.Type == AdjustDurationType.Seconds)
        {
            DoAdjustViaSeconds(subtitles);
        }
        else if (SelectedAdjustType.Type == AdjustDurationType.Fixed)
        {
            DoAdjustViaFixed(subtitles);
        }
        else if (SelectedAdjustType.Type == AdjustDurationType.Percent)
        {
            DoAdjustViaPercent(subtitles);
        }
        else if (SelectedAdjustType.Type == AdjustDurationType.Recalculate)
        {
            DoAdjustViaRecalculate(subtitles);
        }
    }

    private void DoAdjustViaSeconds(ObservableCollection<SubtitleLineViewModel> subtitles)
    {
        for (var i = 0; i < subtitles.Count; i++)
        {
            var subtitle = subtitles[i];
            var nextSubtitle = subtitles.GetOrNull(i + 1);
            var newEndTime = subtitle.EndTime + TimeSpan.FromSeconds(AdjustSeconds);
            if (nextSubtitle != null && newEndTime <= nextSubtitle.StartTime)
            {
                subtitle.EndTime = newEndTime;
            }
        }
    }

    private void DoAdjustViaFixed(ObservableCollection<SubtitleLineViewModel> subtitles)
    {
        for (int i = 0; i < subtitles.Count; i++)
        {
            var subtitle = subtitles[i];
            var nextSubtitle = subtitles.GetOrNull(i + 1);
            var adjustment = TimeSpan.FromSeconds(AdjustFixed);
            var newEndTime = subtitle.EndTime + adjustment;

            if (nextSubtitle != null && newEndTime > nextSubtitle.StartTime)
            {
                subtitle.EndTime = nextSubtitle.StartTime;
            }
            else
            {
                subtitle.EndTime = newEndTime;
            }
        }
    }

    private void DoAdjustViaPercent(ObservableCollection<SubtitleLineViewModel> subtitles)
    {
        for (int i = 0; i < subtitles.Count; i++)
        {
            var subtitle = subtitles[i];
            var nextSubtitle = subtitles.GetOrNull(i + 1);

            var originalDuration = subtitle.EndTime - subtitle.StartTime;
            var adjustment = originalDuration.TotalSeconds * (AdjustPercent / 100.0);
            var newEndTime = subtitle.EndTime + TimeSpan.FromSeconds(adjustment);

            if (nextSubtitle != null && newEndTime > nextSubtitle.StartTime)
            {
                subtitle.EndTime = nextSubtitle.StartTime;
            }
            else
            {
                subtitle.EndTime = newEndTime;
            }
        }
    }

    private void DoAdjustViaRecalculate(ObservableCollection<SubtitleLineViewModel> subtitles)
    {
        for (int i = 0; i < subtitles.Count; i++)
        {
            var subtitle = subtitles[i];
            var charCount = subtitle.Text?.Length ?? 0;

            var optimalDuration = TimeSpan.FromSeconds(charCount / AdjustRecalculateOptimalCharacterPerSecond);
            var maxDuration = TimeSpan.FromSeconds(charCount / AdjustRecalculateMaxCharacterPerSecond);

            var nextSubtitle = subtitles.GetOrNull(i + 1);
            var maxEndTime = nextSubtitle?.StartTime ?? TimeSpan.MaxValue;

            var proposedEndTime = subtitle.StartTime + optimalDuration;
            var fallbackEndTime = subtitle.StartTime + maxDuration;

            if (proposedEndTime <= maxEndTime)
            {
                subtitle.EndTime = proposedEndTime;
            }
            else if (fallbackEndTime <= maxEndTime)
            {
                subtitle.EndTime = fallbackEndTime;
            }
            else
            {
                subtitle.EndTime = maxEndTime;
            }
        }
    }

    private void LoadSettings()
    {
        AdjustSeconds = Se.Settings.Tools.AdjustDurations.AdjustDurationSeconds;
        AdjustPercent = Se.Settings.Tools.AdjustDurations.AdjustDurationPercent;
        AdjustFixed = Se.Settings.Tools.AdjustDurations.AdjustDurationFixed;
        AdjustRecalculateMaxCharacterPerSecond = Se.Settings.Tools.AdjustDurations.AdjustDurationMaximumCps;
        AdjustRecalculateOptimalCharacterPerSecond = Se.Settings.Tools.AdjustDurations.AdjustDurationOptimalCps;

        SelectedAdjustType = AdjustTypes.FirstOrDefault(p =>
                                 p.Type.ToString() == Se.Settings.Tools.AdjustDurations.AdjustDurationLast)
                             ?? AdjustTypes[0];
        ;
    }

    private void SaveSettings()
    {
        Se.Settings.Tools.AdjustDurations.AdjustDurationSeconds = AdjustSeconds;
        Se.Settings.Tools.AdjustDurations.AdjustDurationPercent = AdjustPercent;
        Se.Settings.Tools.AdjustDurations.AdjustDurationFixed = AdjustFixed;
        Se.Settings.Tools.AdjustDurations.AdjustDurationMaximumCps = AdjustRecalculateMaxCharacterPerSecond;
        Se.Settings.Tools.AdjustDurations.AdjustDurationOptimalCps = AdjustRecalculateOptimalCharacterPerSecond;

        Se.Settings.Tools.AdjustDurations.AdjustDurationLast = SelectedAdjustType.Type.ToString();

        Se.SaveSettings();
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
}