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
using System.Timers;

namespace Nikse.SubtitleEdit.Features.Files.ImportPlainText;

public partial class ImportPlainTextViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;
    [ObservableProperty] private SubtitleLineViewModel? _selectedSubtitle;
    [ObservableProperty] private ObservableCollection<string> _files;
    [ObservableProperty] private string? _selectedFile;
    [ObservableProperty] private ObservableCollection<string> _splitAtOptions;
    [ObservableProperty] private string _selectedSplitAtOption;
    [ObservableProperty] private bool _isImportFilesVisible;
    [ObservableProperty] private bool _isDeleteVisible;
    [ObservableProperty] private bool _isDeleteAllVisible;
    [ObservableProperty] private string _plainText;

    public Window? Window { get; internal set; }
    public bool OkPressed { get; private set; }

    private readonly IFileHelper _fileHelper;
    private readonly List<string> _textExtensions = new List<string>
    {
        "*.txt",
        "*.rtf" ,
    };
    private bool _dirty;
    private readonly System.Timers.Timer _timerUpdatePreview;


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

        _timerUpdatePreview = new Timer();
        _timerUpdatePreview.Interval = 250;
        _timerUpdatePreview.Elapsed += TimerUpdatePreviewElapsed;
        _timerUpdatePreview.Start();
    }

    private void TimerUpdatePreviewElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_dirty)
        {
            var subtitles = new List<SubtitleLineViewModel>();
            if (IsImportFilesVisible)
            {
                subtitles = UpdatePreviewFiles(SelectedSplitAtOption, Files.ToList());
            }
            else
            {
                subtitles = UpdatePreviewText(SelectedSplitAtOption, PlainText);
            }

            subtitles = TimeCodeCalculator.CalculateTimeCodes(
                subtitles, 
                Se.Settings.General.SubtitleOptimalCharactersPerSeconds, 
                Se.Settings.General.SubtitleMaximumCharactersPerSeconds,
                Se.Settings.General.MinimumMillisecondsBetweenLines, 
                Se.Settings.General.SubtitleMinimumDisplayMilliseconds,
                Se.Settings.General.SubtitleMaximumDisplayMilliseconds);

            Dispatcher.UIThread.Post(() =>
            {
                Subtitles.Clear();
                for (int i = 0; i < subtitles.Count; i++)
                {
                    var subtitle = subtitles[i];
                    subtitle.Number = i + 1;
                    Subtitles.Add(subtitle);
                }
            });

            _dirty = false;
        }
    }

    private List<SubtitleLineViewModel> UpdatePreviewFiles(string splitAtOption, List<string> list)
    {
        return new List<SubtitleLineViewModel>();
    }

    private static List<SubtitleLineViewModel> UpdatePreviewText(string splitAtOption, string plainText)
    {
        var subtitles = new List<SubtitleLineViewModel>();

        if (splitAtOption == Se.Language.General.Auto)
        {
            return PlainTextSplitter.AutomaticSplit(plainText, Se.Settings.General.MaxNumberOfLines, Se.Settings.General.SubtitleLineMaximumLength);
        }
        else if (splitAtOption == Se.Language.File.Import.BlankLines)
        {
            var blocks = plainText.SplitToLines();
            foreach (var block in blocks)
            {
                if (!string.IsNullOrWhiteSpace(block))
                {
                    subtitles.Add(new SubtitleLineViewModel { Text = block.Trim() });
                }
            }
        }
        else if (splitAtOption == Se.Language.File.Import.OneLineIsOneSubtitle)
        {
            var lines = plainText.SplitToLines();
            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    subtitles.Add(new SubtitleLineViewModel { Text = line.Trim() });
                }
            }
        }
        else if (splitAtOption == Se.Language.File.Import.TwoLinesAreOneSubtitle)
        {
            var lines = plainText.SplitToLines();
            for (int i = 0; i < lines.Count; i += 2)
            {
                var text = lines[i].Trim();
                if (i + 1 < lines.Count)
                {
                    text += Environment.NewLine + lines[i + 1].Trim();
                }
                if (!string.IsNullOrWhiteSpace(text))
                {
                    subtitles.Add(new SubtitleLineViewModel { Text = text });
                }
            }
        }

        return subtitles;
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

        var text = await File.ReadAllTextAsync(fileName);
        PlainText = text;
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

    internal void PlainTextChanged()
    {
        _dirty = true;
    }

    internal void SplitAtOptionChanged()
    {
        _dirty = true;
    }
}
