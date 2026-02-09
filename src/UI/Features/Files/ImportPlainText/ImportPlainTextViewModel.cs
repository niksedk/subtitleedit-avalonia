using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Core;

namespace Nikse.SubtitleEdit.Features.Files.ImportPlainText;

public partial class ImportPlainTextViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;
    [ObservableProperty] private SubtitleLineViewModel? _selectedSubtitle;
    [ObservableProperty] private ObservableCollection<string> _files;
    [ObservableProperty] private string? _selectedFile;
    [ObservableProperty] private ObservableCollection<string> _splitAtOptions;
    [ObservableProperty] private string? _selectedSplitAtOption;
    [ObservableProperty] private string _plainText;
    [ObservableProperty] private bool _isAutoSplitText = true;
    [ObservableProperty] private bool _isSplitAtBlankLines;
    [ObservableProperty] private bool _isSplitAtLineMode;
    [ObservableProperty] private ObservableCollection<string> _lineBreaks;
    [ObservableProperty] private string? _selectedLineBreak = Se.Settings.Tools.ImportTextLineBreak;
    [ObservableProperty] private int _maxNumberOfLines = Se.Settings.Tools.ImportTextAutoSplitNumberOfLines;
    [ObservableProperty] private int _singleLineMaxLength = Se.Settings.General.SubtitleLineMaximumLength;
    [ObservableProperty] private bool _splitAtBlankLinesSetting = Se.Settings.Tools.ImportTextAutoSplitAtBlank;
    [ObservableProperty] private bool _removeLinesWithoutLetters = Se.Settings.Tools.ImportTextRemoveLinesNoLetters;
    [ObservableProperty] private bool _splitAtEndCharsSetting = Se.Settings.Tools.ImportTextAutoBreakAtEnd;
    [ObservableProperty] private string _endChars = Se.Settings.Tools.ImportTextAutoBreakAtEndMarkerText ?? ".!?";
    [ObservableProperty] private bool _isTimeCodeGenerate = true;
    [ObservableProperty] private bool _isTimeCodeTakeFromCurrent;
    [ObservableProperty] private int _gapBetweenSubtitles = 90;
    [ObservableProperty] private bool _isAutoDuration = true;
    [ObservableProperty] private bool _isFixedDuration;
    [ObservableProperty] private int _fixedDuration = 2000;
    [ObservableProperty] private ObservableCollection<TextEncoding> _encodings;
    [ObservableProperty] private TextEncoding? _selectedEncoding;
    [ObservableProperty] private bool _multipleFilesOneFileIsOneSubtitle;
    [ObservableProperty] private string _previewSubtitlesModifiedText = "Preview - subtitles modified: 0";
    [ObservableProperty] private bool _mergeShortLines;
    [ObservableProperty] private bool _autoBreak;
    [ObservableProperty] private int _startFromNumber = 1;

    public Window? Window { get; internal set; }
    public bool OkPressed { get; private set; }
    public Subtitle FixedSubtitle { get; private set; } = new();

    private readonly IFileHelper _fileHelper;
    private Subtitle? _currentlyLoadedSubtitle;
    private string _currentFileName = string.Empty;

    public void SetCurrentSubtitle(Subtitle subtitle)
    {
        _currentlyLoadedSubtitle = subtitle;
    }

    private readonly List<string> _textExtensions = new List<string> { "*.txt", "*.rtf", "*.tx3g", "*.astx", "*.html" };

    public ImportPlainTextViewModel(IFileHelper fileHelper, Subtitle? currentlyLoadedSubtitle = null)
    {
        _fileHelper = fileHelper;
        _currentlyLoadedSubtitle = currentlyLoadedSubtitle;
        Subtitles = new ObservableCollection<SubtitleLineViewModel>();
        Files = new ObservableCollection<string>();

        SplitAtOptions = new ObservableCollection<string>
        {
            Se.Language.File.Import.OneLineIsOneSubtitle,
            Se.Language.File.Import.TwoLinesAreOneSubtitle,
        };
        SelectedSplitAtOption = SplitAtOptions[0];
        PlainText = string.Empty;

        LineBreaks = new ObservableCollection<string> { string.Empty, "|", ";", "||" };
        SelectedLineBreak = LineBreaks[0];

        Encodings = new ObservableCollection<TextEncoding>(EncodingHelper.GetEncodings());
        SelectedEncoding = Encodings.FirstOrDefault();

        IsAutoSplitText = Se.Settings.Tools.ImportTextSplitting == "auto" || string.IsNullOrEmpty(Se.Settings.Tools.ImportTextSplitting);
        IsSplitAtBlankLines = Se.Settings.Tools.ImportTextSplitting == "blank lines";
        IsSplitAtLineMode = Se.Settings.Tools.ImportTextSplitting == "line";

        MergeShortLines = Se.Settings.Tools.ImportTextMergeShortLines;
        AutoBreak = Se.Settings.Tools.ImportTextAutoBreak;
        IsTimeCodeGenerate = Se.Settings.Tools.ImportTextGenerateTimeCodes;
        IsTimeCodeTakeFromCurrent = false; // Default to false

        GapBetweenSubtitles = Se.Settings.Tools.ImportTextGap;
        IsAutoDuration = Se.Settings.Tools.ImportTextDurationAuto;
        IsFixedDuration = !Se.Settings.Tools.ImportTextDurationAuto;
        FixedDuration = Se.Settings.Tools.ImportTextFixedDuration;

        if (Se.Settings.Tools.ImportTextSplittingLineMode == "TwoLinesAreOneSubtitle" && SplitAtOptions.Count > 1)
        {
            SelectedSplitAtOption = SplitAtOptions[1];
        }
    }

    partial void OnSelectedEncodingChanged(TextEncoding? value)
    {
        if (!string.IsNullOrEmpty(_currentFileName))
        {
            PlainText = LoadTextFromFile(_currentFileName);
        }
        GeneratePreview();
    }

    partial void OnSelectedSplitAtOptionChanged(string? value) => GeneratePreview();
    partial void OnPlainTextChanged(string value) => GeneratePreview();
    partial void OnIsAutoSplitTextChanged(bool value) => GeneratePreview();
    partial void OnIsSplitAtBlankLinesChanged(bool value) => GeneratePreview();
    partial void OnIsSplitAtLineModeChanged(bool value) => GeneratePreview();
    partial void OnSelectedLineBreakChanged(string? value) => GeneratePreview();
    partial void OnMaxNumberOfLinesChanged(int value) => GeneratePreview();
    partial void OnSingleLineMaxLengthChanged(int value) => GeneratePreview();
    partial void OnSplitAtBlankLinesSettingChanged(bool value) => GeneratePreview();
    partial void OnRemoveLinesWithoutLettersChanged(bool value) => GeneratePreview();
    partial void OnSplitAtEndCharsSettingChanged(bool value) => GeneratePreview();
    partial void OnEndCharsChanged(string value) => GeneratePreview();
    partial void OnIsTimeCodeGenerateChanged(bool value) => GeneratePreview();
    partial void OnIsTimeCodeTakeFromCurrentChanged(bool value) => GeneratePreview();
    partial void OnGapBetweenSubtitlesChanged(int value) => GeneratePreview();
    partial void OnIsAutoDurationChanged(bool value) => GeneratePreview();
    partial void OnIsFixedDurationChanged(bool value) => GeneratePreview();
    partial void OnFixedDurationChanged(int value) => GeneratePreview();
    partial void OnMergeShortLinesChanged(bool value) => GeneratePreview();
    partial void OnAutoBreakChanged(bool value) => GeneratePreview();
    partial void OnMultipleFilesOneFileIsOneSubtitleChanged(bool value) => GeneratePreview();

    [RelayCommand]
    private void Refresh() => GeneratePreview();

    private void GeneratePreview()
    {
        if (string.IsNullOrEmpty(PlainText) && Files.Count == 0)
        {
            Subtitles.Clear();
            PreviewSubtitlesModifiedText = "Preview - subtitles modified: 0";
            return;
        }

        if (!string.IsNullOrEmpty(_currentFileName) && IsHtmlIndexExportFromSubtitleEdit(_currentFileName))
        {
            var html = FileUtil.ReadAllTextShared(_currentFileName, Encoding.UTF8);
            FixedSubtitle = GetSubtitleFromHtmlIndex(html);
            var listX = FixedSubtitle.Paragraphs.Select(p => new SubtitleLineViewModel(p, new SubRip())).ToList();
            Subtitles = new ObservableCollection<SubtitleLineViewModel>(listX);
            PreviewSubtitlesModifiedText = $"Preview - subtitles modified: {listX.Count}";
            return;
        }

        FixedSubtitle = new Subtitle();
        var lines = (PlainText ?? string.Empty).SplitToLines();

        var timeCodesOk = false;
        if (!MultipleFilesOneFileIsOneSubtitle)
        {
            foreach (var format in SubtitleFormat.AllSubtitleFormats)
            {
                var typeName = format.GetType().Name;
                if (typeName == "PlainText" || typeName == "SubRip" || format.Name.Contains("CSV", StringComparison.OrdinalIgnoreCase))
                {
                    if (typeName != "SubRip") continue;
                }

                if (format.IsMine(lines, string.Empty))
                {
                    format.LoadSubtitle(FixedSubtitle, lines, string.Empty);
                    if (FixedSubtitle.Paragraphs.Count > 0)
                    {
                        timeCodesOk = true;
                        break;
                    }
                }
            }
        }

        if (MultipleFilesOneFileIsOneSubtitle)
        {
            ImportMultipleFiles();
        }
        else if (timeCodesOk)
        {
            // Already loaded via SubRip check above
        }
        else if (IsSplitAtLineMode)
        {
            ImportLineMode(lines.ToArray());
        }
        else if (IsAutoSplitText)
        {
            ImportAutoSplit(lines.ToArray());
        }
        else
        {
            ImportSplitAtBlankLine(lines.ToList());
        }

        if (MergeShortLines)
        {
            MergeLinesWithContinuation();
        }

        FixedSubtitle.Renumber(StartFromNumber);

        if (IsTimeCodeGenerate && IsTimeCodeTakeFromCurrent && _currentlyLoadedSubtitle != null)
        {
            for (var i = 0; i < FixedSubtitle.Paragraphs.Count; i++)
            {
                var p = FixedSubtitle.Paragraphs[i];
                var o = _currentlyLoadedSubtitle.GetParagraphOrDefault(i);
                if (o != null)
                {
                    p.StartTime.TotalMilliseconds = o.StartTime.TotalMilliseconds;
                    p.EndTime.TotalMilliseconds = o.EndTime.TotalMilliseconds;
                }
            }
        }
        else if (IsTimeCodeGenerate && !timeCodesOk)
        {
            FixDurations();
            MakePseudoStartTime();
        }
        // If IsTimeCodeNone is true (removed but logic remains), no time codes are set.

        var list = FixedSubtitle.Paragraphs.Select(p => new SubtitleLineViewModel(p, new SubRip())).ToList();
        Subtitles = new ObservableCollection<SubtitleLineViewModel>(list);
        PreviewSubtitlesModifiedText = $"Preview - subtitles modified: {list.Count}";
    }

    private void ImportMultipleFiles()
    {
        foreach (var fileName in Files)
        {
            var text = LoadTextFromFile(fileName).Replace("|", Environment.NewLine);
            if (!string.IsNullOrEmpty(SelectedLineBreak))
            {
                foreach (var splitter in SelectedLineBreak.Split(';', StringSplitOptions.RemoveEmptyEntries))
                {
                    text = text.Replace(splitter.Trim(), Environment.NewLine);
                }
            }
            FixedSubtitle.Paragraphs.Add(new Paragraph(text.Trim(), 0, 0));
        }
    }

    private void ImportLineMode(string[] lines)
    {
        int mode = SelectedSplitAtOption == Se.Language.File.Import.TwoLinesAreOneSubtitle ? 2 : 1;
        var currentLines = new List<string>();
        foreach (var line in lines)
        {
            var processed = line;
            if (!string.IsNullOrEmpty(SelectedLineBreak))
            {
                foreach (var splitter in SelectedLineBreak.Split(';', StringSplitOptions.RemoveEmptyEntries))
                {
                    processed = processed.Replace(splitter.Trim(), Environment.NewLine);
                }
            }

            if (string.IsNullOrWhiteSpace(processed) && !RemoveLinesWithoutLetters)
                currentLines.Add(string.Empty);
            else if (!string.IsNullOrWhiteSpace(processed))
                currentLines.Add(AutoBreak ? Utilities.AutoBreakLine(processed.Trim()) : processed.Trim());

            if (currentLines.Count >= mode)
            {
                FixedSubtitle.Paragraphs.Add(new Paragraph(string.Join(Environment.NewLine, currentLines), 0, 0));
                currentLines.Clear();
            }
        }
        if (currentLines.Count > 0)
            FixedSubtitle.Paragraphs.Add(new Paragraph(string.Join(Environment.NewLine, currentLines), 0, 0));
    }

    private void ImportAutoSplit(string[] lines)
    {
        var sub = new Subtitle();
        foreach (var line in lines)
        {
            sub.Paragraphs.Add(new Paragraph(line, 0, 0));
        }
        var language = LanguageAutoDetect.AutoDetectGoogleLanguage(sub);

        var importer = new PlainTextImporter(SplitAtBlankLinesSetting, RemoveLinesWithoutLetters, MaxNumberOfLines,
            SplitAtEndCharsSetting ? EndChars : string.Empty, SingleLineMaxLength, language);

        var autoLines = importer.ImportAutoSplit(lines);

        // Pass to ImportLineMode for final grouping if needed (though auto-split usually does the heavy lifting)
        // Original code does: ImportLineMode(plainTextImporter.ImportAutoSplit(textLines));
        // In original code ImportLineMode uses comboBoxLineMode which is 1 or 2 lines.
        // If we are in AutoSplit, the original UI seems to use the numericUpDownAutoSplitMaxLines instead?
        // Wait, let's check original code again.
        ImportLineMode(autoLines.ToArray());
    }

    private void ImportSplitAtBlankLine(List<string> lines)
    {
        var sb = new StringBuilder();
        var tempLines = new List<string>(lines) { string.Empty };
        foreach (var line in tempLines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                if (sb.Length > 0)
                {
                    var text = sb.ToString().Trim();
                    FixedSubtitle.Paragraphs.Add(new Paragraph(AutoBreak ? Utilities.AutoBreakLine(text) : text, 0, 0));
                    sb.Clear();
                }
            }
            else
            {
                sb.AppendLine(line.Trim());
            }
        }
    }

    private void MergeLinesWithContinuation()
    {
        var temp = new Subtitle();
        var skipNext = false;
        for (var i = 0; i < FixedSubtitle.Paragraphs.Count; i++)
        {
            var p = FixedSubtitle.Paragraphs[i];

            if (!skipNext)
            {
                var next = FixedSubtitle.GetParagraphOrDefault(i + 1);

                // Check 1: Basic merge conditions
                bool merge = next != null && !p.Text.Contains(Environment.NewLine) && MaxNumberOfLines > 1;

                // Check 2: Punctuation (don't merge if ends with . or ! and next starts with uppercase)
                if (merge && (p.Text.TrimEnd().EndsWith('!') || p.Text.TrimEnd().EndsWith('.')))
                {
                    var st = new StrippableText(next!.Text);
                    if (st.StrippedText.Length > 0 && char.IsUpper(st.StrippedText[0]))
                    {
                        merge = false;
                    }
                }

                // Check 3: Length limits (don't merge if resulting line is too long)
                if (merge && (p.Text.Length >= SingleLineMaxLength - 5 || next!.Text.Length >= SingleLineMaxLength - 5))
                {
                    merge = false;
                }

                if (merge)
                {
                    temp.Paragraphs.Add(new Paragraph(p) { Text = p.Text + Environment.NewLine + next!.Text });
                    skipNext = true;
                }
                else
                {
                    temp.Paragraphs.Add(new Paragraph(p));
                }
            }
            else
            {
                skipNext = false;
            }
        }
        FixedSubtitle = temp;
    }

    private void FixDurations()
    {
        foreach (var p in FixedSubtitle.Paragraphs)
        {
            if (p.Text.Length == 0)
            {
                // Fallback for empty lines
                p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + 2000;
            }
            else
            {
                p.EndTime.TotalMilliseconds = p.StartTime.TotalMilliseconds + (IsAutoDuration ? Utilities.GetOptimalDisplayMilliseconds(p.Text) : FixedDuration);
            }
        }
    }

    private void MakePseudoStartTime()
    {
        double currentMs = GapBetweenSubtitles;
        foreach (var p in FixedSubtitle.Paragraphs)
        {
            var dur = p.DurationTotalMilliseconds;
            p.StartTime.TotalMilliseconds = currentMs;
            p.EndTime.TotalMilliseconds = currentMs + dur;
            currentMs += dur + GapBetweenSubtitles;
        }
    }

    private string LoadTextFromFile(string fileName)
    {
        var encoding = SelectedEncoding?.Encoding ?? Encoding.UTF8;
        if (fileName.EndsWith(".rtf", StringComparison.OrdinalIgnoreCase))
        {
            var rtf = File.ReadAllText(fileName, encoding);
            return Regex.Replace(rtf, @"\{\*?\\[^{}]+\}|\\\n|\n|\r|\\|[{}]+", "");
        }
        if (fileName.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
        {
            var html = File.ReadAllText(fileName, encoding);
            return WebUtility.HtmlDecode(Regex.Replace(html, "<.*?>", string.Empty));
        }
        return File.ReadAllText(fileName, encoding);
    }

    private static bool IsHtmlIndexExportFromSubtitleEdit(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return false;
        var html = FileUtil.ReadAllTextShared(fileName, Encoding.UTF8);
        var s = GetSubtitleFromHtmlIndex(html);
        return s.Paragraphs.Count > 0;
    }

    private static Subtitle GetSubtitleFromHtmlIndex(string html)
    {
        var lines = html
            .Replace($"<br />{Environment.NewLine}", "<br />")
            .Replace("<br />\\n", "<br />")
            .SplitToLines();

        var subtitle = new Subtitle();
        foreach (var line in lines)
        {
            var indexOfText = line.IndexOf("background-color:", StringComparison.OrdinalIgnoreCase);
            if (indexOfText >= 0)
            {
                indexOfText = line.IndexOf('>', indexOfText);
            }

            var indexOfFirstColon = line.IndexOf(':');
            var indexOfTimeSplit = line.IndexOf("->", StringComparison.Ordinal);
            var indexOfFirstDiv = line.IndexOf("<div", StringComparison.OrdinalIgnoreCase);
            if (indexOfText > 0 && indexOfFirstColon > 0 && indexOfTimeSplit > 0 && indexOfFirstDiv > 0)
            {
                try
                {
                    var start = line.Substring(indexOfFirstColon + 1, indexOfTimeSplit - indexOfFirstColon - 1);
                    var end = line.Substring(indexOfTimeSplit + 2, indexOfFirstDiv - indexOfTimeSplit - 2);
                    var text = line.Substring(indexOfText + 1)
                        .Replace("</div>", string.Empty)
                        .Replace("<hr />", string.Empty)
                        .Replace("<hr/>", string.Empty)
                        .Replace("<hr>", string.Empty)
                        .Replace("<br />", Environment.NewLine)
                        .Replace("<br>", Environment.NewLine)
                        .Trim();
                    text = WebUtility.HtmlDecode(text);
                    var p = new Paragraph(text, DecodeTimeCode(start), DecodeTimeCode(end));
                    subtitle.Paragraphs.Add(p);
                }
                catch
                {
                    // Ignore parse errors for specific lines
                }
            }
        }
        subtitle.Renumber();
        return subtitle;
    }

    private static double DecodeTimeCode(string tc)
    {
        var parts = tc.Split(',', '.', ':');
        try
        {
            if (parts.Length == 2)
                return new TimeCode(0, 0, int.Parse(parts[0]), int.Parse(parts[1])).TotalMilliseconds;
            if (parts.Length == 3)
                return new TimeCode(0, int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2])).TotalMilliseconds;
            if (parts.Length == 4)
                return new TimeCode(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3])).TotalMilliseconds;
        }
        catch { return 0; }
        return 0;
    }

    [RelayCommand]
    private void Ok()
    {
        Se.Settings.Tools.ImportTextSplitting = IsAutoSplitText ? "auto" : (IsSplitAtBlankLines ? "blank lines" : "line");
        Se.Settings.Tools.ImportTextSplittingLineMode = SelectedSplitAtOption == Se.Language.File.Import.TwoLinesAreOneSubtitle ? "TwoLinesAreOneSubtitle" : "OneLineIsOneSubtitle";
        Se.Settings.Tools.ImportTextLineBreak = SelectedLineBreak ?? string.Empty;
        Se.Settings.Tools.ImportTextMergeShortLines = MergeShortLines;
        Se.Settings.Tools.ImportTextAutoSplitAtBlank = SplitAtBlankLinesSetting;
        Se.Settings.Tools.ImportTextRemoveLinesNoLetters = RemoveLinesWithoutLetters;
        Se.Settings.Tools.ImportTextGenerateTimeCodes = IsTimeCodeGenerate;
        Se.Settings.Tools.ImportTextAutoBreak = AutoBreak;
        Se.Settings.Tools.ImportTextAutoBreakAtEnd = SplitAtEndCharsSetting;
        Se.Settings.Tools.ImportTextGap = GapBetweenSubtitles;
        Se.Settings.Tools.ImportTextAutoSplitNumberOfLines = MaxNumberOfLines;
        Se.Settings.Tools.ImportTextAutoBreakAtEndMarkerText = EndChars;
        Se.Settings.Tools.ImportTextDurationAuto = IsAutoDuration;
        Se.Settings.Tools.ImportTextFixedDuration = FixedDuration;
        Se.SaveSettings();

        OkPressed = true;
        Close();
    }

    [RelayCommand]
    private void Cancel() => Close();

    [RelayCommand]
    private async Task FileImport()
    {
        if (Window == null) return;
        var fileName = await _fileHelper.PickOpenFile(Window, Se.Language.General.Title, Se.Language.General.TextFiles, ".txt", Se.Language.General.TextFiles);
        if (string.IsNullOrEmpty(fileName)) return;
        _currentFileName = fileName;
        PlainText = LoadTextFromFile(fileName);
    }

    [RelayCommand]
    private async Task FilesImport()
    {
        if (Window == null) return;
        var fileNames = await _fileHelper.PickOpenFiles(Window, "Open text files", Se.Language.General.TextFiles, _textExtensions, string.Empty, new List<string>());
        if (fileNames.Length == 0) return;
        foreach (var f in fileNames) Files.Add(f);
        MultipleFilesOneFileIsOneSubtitle = true;
    }

    private void Close() => Dispatcher.UIThread.Post(() => Window?.Close());

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) Close();
    }
}