using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Files.ExportCustomTextFormat;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Files.ExportPlainText;

public partial class ExportPlainTextViewModel : ObservableObject
{
    [ObservableProperty] private bool _formatTextNone;
    [ObservableProperty] private bool _formatTextMerge;
    [ObservableProperty] private bool _formatTextUnbreak;
    [ObservableProperty] private bool _formatTextRemoveStyling;
    [ObservableProperty] private bool _showLineNumbers;
    [ObservableProperty] private bool _addNewLineAfterLineNumber;
    [ObservableProperty] private bool _showTimeCodes;
    [ObservableProperty] private bool _addNewLineAfterTimeCode;
    [ObservableProperty] private ObservableCollection<string> _timeCodeFormats;
    [ObservableProperty] private string _selectedTimeCodeFormats;
    [ObservableProperty] private ObservableCollection<string> _timeCodeSeparators;
    [ObservableProperty] private string _selectedTimeCodeSeparator;
    [ObservableProperty] private bool _addLineAfterText;
    [ObservableProperty] private bool _addLineBetweenSubtitles;

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

    public ExportPlainTextViewModel(IWindowService windowService, IFileHelper fileHelper)
    {
        _windowService = windowService;
        _fileHelper = fileHelper;

        _title = string.Empty;

        TimeCodeFormats = new ObservableCollection<string>
        {
            "hh:mm:ss,zzz",
            "hh:mm:ss.zzz",
            "hh:mm:ss:ff",
            "mm:ss,zzz",
            "mm:ss.zzz",
            "mm:ss:ff",
            "ss,zzz",
            "ss.zzz",
            "ss:ff"
        };
        SelectedTimeCodeFormats = TimeCodeFormats[0];

        TimeCodeSeparators = new ObservableCollection<string>
        {
            " - ",
            " --> ",
            " > ",
            " ~ ",
            " | ",
            " / ",
            " \\ ",
            ": ",
            " -",
            "- ",
            "-",
            " "
        };
        SelectedTimeCodeSeparator = TimeCodeSeparators[0];


        Encodings = new ObservableCollection<TextEncoding>(EncodingHelper.GetEncodings());
        PreviewText = string.Empty;
        _subtitles = new List<SubtitleLineViewModel>();
    }


    [RelayCommand]
    private async Task SaveAs()
    {
        //if (SelectedCustomFormat == null || Window == null)
        //{
        //    return;
        //}

        //var fileName = await _fileHelper.PickSaveFile(Window, SelectedCustomFormat.Extension, _title, Se.Language.General.SaveFileAsTitle);
        //if (string.IsNullOrWhiteSpace(fileName))
        //{
        //    return;
        //}

        //System.IO.File.WriteAllText(fileName, PreviewText); // TODO: use default encoding
    }

    [RelayCommand]
    private void Ok()
    {
        //Se.Settings.File.ExportCustomFormats.Clear();
        //foreach (var item in CustomFormats)
        //{
        //    Se.Settings.File.ExportCustomFormats.Add(item.ToCustomFormat());
        //}

        OkPressed = true;
        Window?.Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Window?.Close();
    }

    internal void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            Window?.Close();
        }
    }

  

    internal void Initialize(List<SubtitleLineViewModel> subtitles, string? subtitleFileName, string? videoFileName)
    {
    }
}