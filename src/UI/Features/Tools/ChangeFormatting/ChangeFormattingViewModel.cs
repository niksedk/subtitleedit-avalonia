using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Tools.ChangeFormatting;

public partial class ChangeFormattingViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<ChangeFormattingDisplayItem> _subtitles;
    [ObservableProperty] private ChangeFormattingDisplayItem? _selectedSubtitle;
    [ObservableProperty] private ObservableCollection<ChangeFormattingTypeDisplay> _fromTypes;
    [ObservableProperty] private ChangeFormattingTypeDisplay? _selectedFromType;
    [ObservableProperty] private ObservableCollection<ChangeFormattingTypeDisplay> _toTypes;
    [ObservableProperty] private ChangeFormattingTypeDisplay? _selectedToType;
    [ObservableProperty] private int _bridgeGapsSmallerThanMs;
    [ObservableProperty] private int _minGapMs;
    [ObservableProperty] private int _percentForLeft;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _allSubtitles;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public List<SubtitleLineViewModel> FixedSubtitle { get; set; }

    private readonly System.Timers.Timer _timerUpdatePreview;
    private bool _dirty;
    private Dictionary<string, string> _dic;


    public ChangeFormattingViewModel()
    {
        Subtitles = new ObservableCollection<ChangeFormattingDisplayItem>();
        FromTypes = new ObservableCollection<ChangeFormattingTypeDisplay>(ChangeFormattingTypeDisplay.GetFromTypes());
        ToTypes = new ObservableCollection<ChangeFormattingTypeDisplay>(ChangeFormattingTypeDisplay.GetToTypes());
        AllSubtitles = new ObservableCollection<SubtitleLineViewModel>();
        FixedSubtitle = new List<SubtitleLineViewModel>();
        BridgeGapsSmallerThanMs = 500;
        MinGapMs = 10;
        StatusText = string.Empty;
        _dic = new Dictionary<string, string>();

        LoadSettings();

        _timerUpdatePreview = new System.Timers.Timer(500);
        _timerUpdatePreview.Elapsed += (s, e) =>
        {
            _timerUpdatePreview.Stop();
            if (_dirty)
            {
                _dirty = false;
                UpdatePreview();
            }
            _timerUpdatePreview.Start();
        };
    }

    private void UpdatePreview()
    {
        if (SelectedFromType == null || SelectedToType == null)
        {
            return;
        }

        var allSubtitles = new ObservableCollection<SubtitleLineViewModel>(AllSubtitles.Select(p => new SubtitleLineViewModel(p)));
        var fixedCount = 0;
        Dispatcher.UIThread.Post(() =>
        {
            Subtitles.Clear();
            foreach (var v in allSubtitles)
            {
                var vm = new ChangeFormattingDisplayItem(v);
                vm.NewText = FormattingReplacer.Replace(v.Text, SelectedFromType.Type, SelectedToType.Type, Colors.Yellow, new SubRip());
                if (vm.Text != vm.NewText)
                {
                    fixedCount++;
                }
                Subtitles.Add(vm);
            }

            StatusText = string.Format(Se.Language.Tools.BridgeGaps.NumberOfSmallGapsBridgedX, fixedCount);
        });
    }

    public void Initialize(List<SubtitleLineViewModel> subtitles)
    {
        AllSubtitles.Clear();
        AllSubtitles.AddRange(subtitles.Select(p => new SubtitleLineViewModel(p)));
        _dirty = true;
        _timerUpdatePreview.Start();
    }

    private void LoadSettings()
    {
        //BridgeGapsSmallerThanMs = Se.Settings.Tools.BridgeGaps.BridgeGapsSmallerThanMs;
        //MinGapMs = Se.Settings.Tools.BridgeGaps.MinGapMs;
        //PercentForLeft = Se.Settings.Tools.BridgeGaps.PercentForLeft;
    }

    private void SaveSettings()
    {
        //Se.Settings.Tools.BridgeGaps.BridgeGapsSmallerThanMs = BridgeGapsSmallerThanMs;
        //Se.Settings.Tools.BridgeGaps.MinGapMs = MinGapMs;
        //Se.Settings.Tools.BridgeGaps.PercentForLeft = PercentForLeft;

        Se.SaveSettings();
    }

    [RelayCommand]
    private void Ok()
    {
        FixedSubtitle = Subtitles.Select(p => new SubtitleLineViewModel(p.SubtitleLineViewModel)).ToList();
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

    internal void ValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        _dirty = true;
    }

    internal void SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _dirty = true;
    }
}