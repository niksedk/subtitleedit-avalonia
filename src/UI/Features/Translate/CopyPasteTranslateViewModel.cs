using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Translate;

public partial class CopyPasteTranslateViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;
    [ObservableProperty] private SubtitleLineViewModel? _selectedSubtitle;
    [ObservableProperty] private int? _maxBlockSize;
    [ObservableProperty] private string _lineSeparator;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }
    public DataGrid SubtitleGrid { get; internal set; }

    private readonly IWindowService _windowService;

    public CopyPasteTranslateViewModel(IWindowService windowService)
    {
        _windowService = windowService;

        SubtitleGrid = new DataGrid();
        Subtitles = new ObservableCollection<SubtitleLineViewModel>();
        MaxBlockSize = Se.Settings.AutoTranslate.CopyPasteMaxBlockSize;
        LineSeparator = Se.Settings.AutoTranslate.CopyPasteLineSeparator;
    }

    private void SaveSettings()
    {
        if (MaxBlockSize.HasValue)
        {
            Se.Settings.AutoTranslate.CopyPasteMaxBlockSize = MaxBlockSize.Value;
        }

        Se.Settings.AutoTranslate.CopyPasteLineSeparator = LineSeparator ?? ".";

        Se.SaveSettings();
    }

    internal void Initialize(List<SubtitleLineViewModel> subtitles)
    {
        Subtitles.Clear();
        
        foreach (var s in subtitles)
        {
            var s2 = new SubtitleLineViewModel(s);
            s2.OriginalText = s.Text;
            s2.Text = s.OriginalText;
            Subtitles.Add(s2);
        }

        if (Subtitles.Count > 0)
        {
            SelectedSubtitle = Subtitles[0];
        }
    }

    [RelayCommand]
    private async Task Translate()
    {
        if (Window == null)
        {
            return;
        }

        await _windowService.ShowDialogAsync<CopyPasteTranslateBlockWindow, CopyPasteTranslateBlockViewModel>(Window!, vm =>
        {
        });
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

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }
}