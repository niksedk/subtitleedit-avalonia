using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Files.ExportCustomTextFormat;

public partial class ExportCustomTextFormatViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<CustomFormatItem> _customFormats;
    [ObservableProperty] private CustomFormatItem? _selectedCustomFormat;

    [ObservableProperty] private ObservableCollection<TextEncoding> _encodings;
    [ObservableProperty] private TextEncoding? _selectedEncoding;

    [ObservableProperty] private string _previewText;

    private List<SubtitleLineViewModel> _subtitles;
    private string? _subtitleFileNAme;
    private string? _videoFileName;
    private string _title;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }

    private IWindowService _windowService;

    public ExportCustomTextFormatViewModel(IWindowService windowService)
    {
        _windowService = windowService;
        _title = string.Empty;  
        CustomFormats = new ObservableCollection<CustomFormatItem>();
        Encodings = new ObservableCollection<TextEncoding>(EncodingHelper.GetEncodings());
        PreviewText = string.Empty;
        _subtitles = new List<SubtitleLineViewModel>();
        LoadSettings();
    }

    private void LoadSettings()
    {
    }

    private void SaveSettings()
    {
        Se.SaveSettings();
    }

    [RelayCommand]
    private async Task FormatEdit()
    {
        var selected = SelectedCustomFormat;
        if (selected == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<EditCustomTextFormatWindow, EditCustomTextFormatViewModel>(Window!, vm =>
        {
            vm.Initialize(selected, Se.Language.File.Export.EditCustomFormat);
        });

    }

    [RelayCommand]
    private void FormatDelete()
    {
    }

    [RelayCommand]
    private async Task FormatNew()
    {
        var selected = SelectedCustomFormat;
        if (selected == null)
        {
            return;
        }

        var result = await _windowService.ShowDialogAsync<EditCustomTextFormatWindow, EditCustomTextFormatViewModel>(Window!, vm =>
        {
            vm.Initialize(selected, Se.Language.File.Export.NewCustomFormat);
        });
    }

    [RelayCommand]
    private void SaveAs()
    {
        SaveSettings();
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

    internal void OnCustomFormatGridDoubleTapped(object? sender, TappedEventArgs e)
    {
    }

    internal void GridSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selected = SelectedCustomFormat;
        if (selected == null)
        {
            return;
        }

        GenerateText(selected);
    }

    private void GenerateText(CustomFormatItem customFormatItem)
    {
        var sb = new StringBuilder();
        sb.Append(CustomTextFormatter.GetHeaderOrFooter(_title, _videoFileName ?? string.Empty, _subtitles, customFormatItem.FormatHeader));
        var template = CustomTextFormatter.GetParagraphTemplate(customFormatItem.FormatText);
        var isXml = customFormatItem.FormatText.Contains("<?xml version=", StringComparison.OrdinalIgnoreCase);
        for (var i = 0; i < _subtitles.Count; i++)
        {
            var p = _subtitles[i];
            var start = CustomTextFormatter.GetTimeCode(TimeCode.FromSeconds(p.StartTime.TotalSeconds), customFormatItem.FormatTimeCode);
            var end = CustomTextFormatter.GetTimeCode(TimeCode.FromSeconds(p.EndTime.TotalSeconds), customFormatItem.FormatTimeCode);

            var gap = string.Empty;
            var next = _subtitles.GetOrNull(i + 1);
            if (next != null)
            {
                gap = CustomTextFormatter.GetTimeCode(new TimeCode(next.StartTime.TotalMilliseconds - p.EndTime.TotalMilliseconds), customFormatItem.FormatTimeCode);
            }

            var text = p.Text;
            if (isXml)
            {
                text = text.Replace("<", "&lt;")
                           .Replace(">", "&gt;")
                           .Replace("&", "&amp;");
            }
            text = CustomTextFormatter.GetText(text, customFormatItem.FormatNewLine);

            var originalText = p.OriginalText;
            var paragraph = CustomTextFormatter.GetParagraph(template, start, end, text, originalText, i, p.Actor, new TimeCode(p.Duration), gap, customFormatItem.FormatTimeCode, p, _videoFileName ?? string.Empty);
            sb.Append(paragraph);
        }
        
        sb.Append(CustomTextFormatter.GetHeaderOrFooter(_title, _videoFileName ?? string.Empty, _subtitles, customFormatItem.FormatFooter));
     
        PreviewText = sb.ToString();
    }

    internal void GridKeyDown(KeyEventArgs e)
    {
    }

    internal void Initialize(List<SubtitleLineViewModel> subtitles, string? subtitleFileName, string? videoFileName)
    {
        _subtitles = subtitles;
        _subtitleFileNAme = subtitleFileName;
        _videoFileName = videoFileName;
        _title = subtitleFileName != null ? System.IO.Path.GetFileNameWithoutExtension(subtitleFileName) : Se.Language.General.Untitled;    

        foreach (var customFormat in Se.Settings.File.ExportCustomFormats)
        {
            CustomFormats.Add(new CustomFormatItem(customFormat));
        }
    }
}