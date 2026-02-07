using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.SubtitleFormats;

namespace Nikse.SubtitleEdit.Features.Files.ImportImages;

public partial class ImportPlainTextViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;
    [ObservableProperty] private SubtitleLineViewModel? _selectedSubtitle;
    [ObservableProperty] private ObservableCollection<string> _files;
    [ObservableProperty] private string? _selectedFile;
    [ObservableProperty] private ObservableCollection<string> _splitAtOptions;
    [ObservableProperty] private string? _selectedSplitAtOption;
    [ObservableProperty] private bool _isImportFilesVisible;
    [ObservableProperty] private bool _isDeleteVisible;
    [ObservableProperty] private bool _isDeleteAllVisible;
    [ObservableProperty] private string _plainText;
    [ObservableProperty] private bool _isAutoSplitText = true;
    [ObservableProperty] private bool _isSplitAtBlankLines;
    [ObservableProperty] private ObservableCollection<string> _lineBreaks;
    [ObservableProperty] private string? _selectedLineBreak;
    [ObservableProperty] private int _maxNumberOfLines = 2;
    [ObservableProperty] private int _singleLineMaxLength = 42;
    [ObservableProperty] private bool _splitAtBlankLinesSetting = true;
    [ObservableProperty] private bool _removeLinesWithoutLetters;
    [ObservableProperty] private bool _splitAtEndCharsSetting = true;
    [ObservableProperty] private string _endChars = ".!?";
    [ObservableProperty] private bool _generateTimeCodes = true;
    [ObservableProperty] private bool _takeTimeFromCurrentFile;
    [ObservableProperty] private int _gapBetweenSubtitles = 90;
    [ObservableProperty] private bool _isAutoDuration = true;
    [ObservableProperty] private bool _isFixedDuration;
    [ObservableProperty] private int _fixedDuration = 2500;
    [ObservableProperty] private ObservableCollection<string> _encodings;
    [ObservableProperty] private string? _selectedEncoding;
    [ObservableProperty] private bool _multipleFilesOneFileIsOneSubtitle;
    [ObservableProperty] private string _previewSubtitlesModifiedText = "Preview - subtitles modified: 0";

    public Window? Window { get; internal set; }
    public bool OkPressed { get; private set; }

    private readonly IFileHelper _fileHelper;
    private readonly List<string> _textExtensions = new List<string>
    {
        "*.txt",
        "*.rtf" ,
    };

    public ImportPlainTextViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;
        Subtitles = new ObservableCollection<SubtitleLineViewModel>();
        Files = new ObservableCollection<string>();
        SplitAtOptions = new ObservableCollection<string>
        {
            Se.Language.General.Auto,
            Se.Language.File.Import.BlankLines,
            Se.Language.File.Import.OneLineIsOneSubtitle,
            Se.Language.File.Import.TwoLinesAreOneSubtitle,
        };
        SelectedSplitAtOption = SplitAtOptions[0];
        PlainText = string.Empty;

        LineBreaks = new ObservableCollection<string> { "<br />", "|", "||" };
        SelectedLineBreak = LineBreaks[0];

        Encodings = new ObservableCollection<string> { "UTF-8", "UTF-8 with BOM", "UTF-16", "ANSI" };
        SelectedEncoding = Encodings[1];
    }

    partial void OnSelectedSplitAtOptionChanged(string? value) => GeneratePreview();
    partial void OnPlainTextChanged(string value) => GeneratePreview();
    partial void OnIsAutoSplitTextChanged(bool value) => GeneratePreview();
    partial void OnIsSplitAtBlankLinesChanged(bool value) => GeneratePreview();
    partial void OnSelectedLineBreakChanged(string? value) => GeneratePreview();
    partial void OnMaxNumberOfLinesChanged(int value) => GeneratePreview();
    partial void OnSingleLineMaxLengthChanged(int value) => GeneratePreview();
    partial void OnSplitAtBlankLinesSettingChanged(bool value) => GeneratePreview();
    partial void OnRemoveLinesWithoutLettersChanged(bool value) => GeneratePreview();
    partial void OnSplitAtEndCharsSettingChanged(bool value) => GeneratePreview();
    partial void OnEndCharsChanged(string value) => GeneratePreview();
    partial void OnGenerateTimeCodesChanged(bool value) => GeneratePreview();
    partial void OnGapBetweenSubtitlesChanged(int value) => GeneratePreview();
    partial void OnIsAutoDurationChanged(bool value) => GeneratePreview();
    partial void OnIsFixedDurationChanged(bool value) => GeneratePreview();
    partial void OnFixedDurationChanged(int value) => GeneratePreview();

    [RelayCommand]
    private void Refresh() => GeneratePreview();

    private void GeneratePreview()
    {
        if (string.IsNullOrEmpty(PlainText))
        {
            Subtitles.Clear();
            PreviewSubtitlesModifiedText = "Preview - subtitles modified: 0";
            return;
        }

        var splitAtBlankLines = IsSplitAtBlankLines || (IsAutoSplitText && SplitAtBlankLinesSetting);
        var numberOfLines = -1;
        if (SelectedSplitAtOption == Se.Language.File.Import.OneLineIsOneSubtitle)
        {
            numberOfLines = 1;
        }
        else if (SelectedSplitAtOption == Se.Language.File.Import.TwoLinesAreOneSubtitle)
        {
            numberOfLines = 2;
        }

        var text = PlainText;
        if (!string.IsNullOrEmpty(SelectedLineBreak))
        {
            var delimiters = SelectedLineBreak.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var d in delimiters)
            {
                text = text.Replace(d, Environment.NewLine);
            }
        }

        var endChars = SplitAtEndCharsSetting ? EndChars : string.Empty;
        var importer = new PlainTextImporter(splitAtBlankLines, RemoveLinesWithoutLetters, numberOfLines, endChars, SingleLineMaxLength, Se.Settings.General.Language);
        var lines = importer.ImportAutoSplit(text.SplitToLines());

        var list = new List<SubtitleLineViewModel>();
        double startTime = 0;
        var format = new SubRip();
        foreach (var line in lines)
        {
            var duration = IsAutoDuration ? Utilities.GetOptimalDisplayMilliseconds(line) : FixedDuration;
            var p = new Paragraph(line, startTime, startTime + duration);
            list.Add(new SubtitleLineViewModel(p, format));
            if (GenerateTimeCodes)
            {
                startTime += duration + GapBetweenSubtitles;
            }
        }
        Subtitles = new ObservableCollection<SubtitleLineViewModel>(list);
        PreviewSubtitlesModifiedText = $"Preview - subtitles modified: {list.Count}";
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    [RelayCommand]
    private async Task FileImport()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickOpenFile(Window, Se.Language.General.ChooseImageFiles, Se.Language.General.TextFiles, ".txt", Se.Language.General.TextFiles);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        try
        {
            PlainText = await File.ReadAllTextAsync(fileName);
            // Default split behavior or preserve from UI? 
            // The setter of PlainText will trigger GeneratePreview via OnPlainTextChanged
        }
        catch (Exception ex)
        {
            // Handle error (log or show message? MainViewModel is where we have Window usually)
            // For now just ignore or print to debug
            System.Diagnostics.Debug.WriteLine(ex.Message);
        }
    }

    [RelayCommand]
    private async Task FilesImport()
    {
        if (Window == null)
        {
            return;
        }

        var fileNames = await _fileHelper.PickOpenFiles(Window, Se.Language.General.ChooseImageFiles, Se.Language.General.Images, _textExtensions, string.Empty, new List<string>());
        if (fileNames.Length == 0)
        {
            return;
        }

        foreach (var fileName in fileNames)
        {

        }
    }

    [RelayCommand]
    private void ImageRemove()
    {
        //var selectedStyle = SelectedImage;
        //if (selectedStyle == null)
        //{
        //    return;
        //}

        //Dispatcher.UIThread.Post(async void () =>
        //{
        //    var answer = await MessageBox.Show(
        //    Window!,
        //    "Remove image?",
        //    $"Do you want to remove {selectedStyle.FileName}?",
        //    MessageBoxButtons.YesNoCancel,
        //    MessageBoxIcon.Question);

        //    if (answer != MessageBoxResult.Yes)
        //    {
        //        return;
        //    }

        //    if (selectedStyle != null)
        //    {
        //        var idx = Images.IndexOf(selectedStyle);
        //        Images.Remove(selectedStyle);
        //        SelectedImage = null;
        //        if (Images.Count > 0)
        //        {
        //            if (idx >= Images.Count)
        //            {
        //                idx = Images.Count - 1;
        //            }
        //            SelectedImage = Images[idx];
        //        }
        //    }
        //});
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }

    internal void AttachmentsDataGridKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete && SelectedFile != null)
        {
            ImageRemove();
            e.Handled = true;
        }
    }

    internal void FileGridOnDragOver(object? sender, DragEventArgs e)
    {
        if (e.DataTransfer.Contains(DataFormat.File))
        {
            e.DragEffects = DragDropEffects.Copy; // show copy cursor
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }

        e.Handled = true;
    }

    internal void FileGridOnDrop(object? sender, DragEventArgs e)
    {
        if (!e.DataTransfer.Contains(DataFormat.File))
        {
            return;
        }

        var files = e.DataTransfer.TryGetFiles();
        if (files != null)
        {
            Dispatcher.UIThread.Post(() =>
            {
                foreach (var file in files)
                {
                    var path = file.Path?.LocalPath;
                    if (path != null && File.Exists(path))
                    {
                        var ext = Path.GetExtension(path).ToLowerInvariant();
                        if (!_textExtensions.Any(x => x.EndsWith(ext)))
                        {
                            continue;
                        }

                        //var importImageItem = new ImportImageItem(path);
                        //Images.Add(importImageItem);
                    }
                }
            });
        }
    }
}
