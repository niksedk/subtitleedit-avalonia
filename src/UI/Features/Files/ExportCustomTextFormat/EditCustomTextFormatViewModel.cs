using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using System.Collections.Generic;
using System.Linq;

namespace Nikse.SubtitleEdit.Features.Files.ExportCustomTextFormat;

public partial class EditCustomTextFormatViewModel : ObservableObject
{
    [ObservableProperty] private CustomFormatItem? _selectedCustomFormat;

    [ObservableProperty] private string _previewText;
    [ObservableProperty] private string _title;

    private List<SubtitleLineViewModel> _subtitles;
    private string _subtitleTitle;
    private string? _subtitleFileName;
    private string? _videoFileName;
    private readonly System.Timers.Timer _previewTimer;

    public Window? Window { get; set; }

    public bool OkPressed { get; private set; }

    public EditCustomTextFormatViewModel()
    {
        Title = string.Empty;
        PreviewText = string.Empty;
        _subtitles = new List<SubtitleLineViewModel>();
        _subtitleTitle = string.Empty;

        _previewTimer = new System.Timers.Timer(500);
        _previewTimer.Elapsed += (sender, args) =>
        {
            if (SelectedCustomFormat == null)
            {
                PreviewText = string.Empty;
                return;
            }

            PreviewText = CustomTextFormatter.GenerateCustomText(SelectedCustomFormat, _subtitles, _subtitleTitle, _videoFileName ?? string.Empty);
        };
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        _previewTimer.Stop();
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

    internal void Initialize(CustomFormatItem selected, string title, List<SubtitleLineViewModel> subtitles)
    {
        SelectedCustomFormat = selected;
        Title = title;
        _subtitles = subtitles.Take(50).ToList();
        _previewTimer.Start();
    }
}