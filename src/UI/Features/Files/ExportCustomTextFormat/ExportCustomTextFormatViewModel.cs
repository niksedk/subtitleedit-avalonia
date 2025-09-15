using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    private IFileHelper _fileHelper;

    public ExportCustomTextFormatViewModel(IWindowService windowService, IFileHelper fileHelper)
    {
        _windowService = windowService;
        _fileHelper = fileHelper;

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
            vm.Initialize(selected, Se.Language.File.Export.EditCustomFormat, _subtitles);
        });

    }

    [RelayCommand]
    private async Task FormatDelete()
    {
        var selected = SelectedCustomFormat;
        if (selected == null || Window == null)
        {
            return;
        }

        var result = await MessageBox.Show(
                   Window,
                   Se.Language.General.DeleteCurrentLine,
                   string.Format(Se.Language.File.Export.DeleteSelectedCustomTextFormatX, selected.Name),
                   MessageBoxButtons.YesNo,
                   MessageBoxIcon.Question);

        if (result != MessageBoxResult.Yes)
        {
            return;
        }


        var idx = CustomFormats.IndexOf(selected);
        CustomFormats.Remove(selected);
        if (CustomFormats.Count > 0)
        {
            if (idx >= CustomFormats.Count)
            {
                idx = CustomFormats.Count - 1;
            }
            SelectedCustomFormat = CustomFormats[idx];
        }
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
            vm.Initialize(selected, Se.Language.File.Export.NewCustomFormat, _subtitles);
        });
    }

    [RelayCommand]
    private async Task SaveAs()
    {
        if (SelectedCustomFormat == null || Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickSaveFile(Window, SelectedCustomFormat.Extension, _title, Se.Language.General.SaveFileAsTitle);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        System.IO.File.WriteAllText(fileName, PreviewText); // TODO: use default encoding
    }

    [RelayCommand]
    private void Ok()
    {
        Se.Settings.File.ExportCustomFormats.Clear();
        foreach (var item in CustomFormats)
        {
            Se.Settings.File.ExportCustomFormats.Add(item.ToCustomFormat());
        }

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

    internal void OnCustomFormatGridDoubleTapped(object? sender, TappedEventArgs e)
    {
        e.Handled = true;
        var _ = FormatEdit();
    }

    internal void GridSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selected = SelectedCustomFormat;
        if (selected == null)
        {
            return;
        }

        e.Handled = true;
        GenerateText(selected);
    }

    private void GenerateText(CustomFormatItem customFormatItem)
    {
        PreviewText = CustomTextFormatter.GenerateCustomText(customFormatItem, _subtitles, _title, _videoFileName ?? string.Empty);
    }

    internal async Task GridKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Delete)
        {
            e.Handled = true;
            await FormatDelete();
        }
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

        if (CustomFormats.Count > 0)
        {
            SelectedCustomFormat = CustomFormats[0];
            GenerateText(SelectedCustomFormat);
        }
    }
}