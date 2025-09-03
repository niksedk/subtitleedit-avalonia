using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Skia;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Shared;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Assa;

public partial class AssaStylesViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private ObservableCollection<StyleDisplay> _fileStyles;
    [ObservableProperty] private StyleDisplay? _selectedFileStyle;
    [ObservableProperty] private ObservableCollection<StyleDisplay> _storageStyles;
    [ObservableProperty] private StyleDisplay? _selectedStorageStyle;
    [ObservableProperty] private StyleDisplay? _currentStyle;
    [ObservableProperty] private ObservableCollection<string> _fonts;
    [ObservableProperty] private ObservableCollection<BorderStyleItem> _borderTypes;
    [ObservableProperty] private BorderStyleItem _selectedBorderType;
    [ObservableProperty] private string _currentTitle;
    [ObservableProperty] private bool _isFileStylesFocused;
    [ObservableProperty] private Bitmap? _imagePreview;
    [ObservableProperty] private bool _isDeleteVisible;
    [ObservableProperty] private bool _isDeleteAllVisible;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public string Header { get; set; }
    public DataGrid FileStyleGrid { get; set; }
    public DataGrid StorageStyleGrid { get; set; }

    private readonly IFileHelper _fileHelper;
    private readonly IWindowService _windowService;
    private Subtitle _subtitle;
    private readonly System.Timers.Timer _timerUpdatePreview;

    public AssaStylesViewModel(IFileHelper fileHelper, IWindowService windowService)
    {
        _fileHelper = fileHelper;
        _windowService = windowService;

        Title = string.Empty;
        FileStyles = new ObservableCollection<StyleDisplay>();
        StorageStyles = new ObservableCollection<StyleDisplay>();
        Fonts = new ObservableCollection<string>(FontHelper.GetSystemFonts());
        BorderTypes = new ObservableCollection<BorderStyleItem>(BorderStyleItem.List());
        SelectedBorderType = BorderTypes[0];
        CurrentTitle = string.Empty;
        FileStyleGrid = new DataGrid();
        StorageStyleGrid = new DataGrid();

        Header = string.Empty;
        _subtitle = new Subtitle();

        LoadSettings();

        _timerUpdatePreview = new System.Timers.Timer(500);
        _timerUpdatePreview.Elapsed += (s, e) =>
        {
            _timerUpdatePreview.Stop();
            UpdatePreview();
            _timerUpdatePreview.Start();
        };
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        SaveFileStylesToHeader();
        SaveSettings();
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

        var format = new AdvancedSubStationAlpha();
        var fileName = await _fileHelper.PickOpenFile(Window, "Open subtitle file to import styles from", format.Name, "*" + format.Extension);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var s = Subtitle.Parse(fileName, format);
        var ssaStyles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(s.Header);

        var result = await _windowService.ShowDialogAsync<AssaStylePickerWindow, AssaStylePickerViewModel>(Window, vm =>
        {
            vm.Initialize(ssaStyles.Select(p => new StyleDisplay(p) { IsSelected = true, Name = MakeUniqueName(p.Name, FileStyles) }).ToList());
        });

        var selectedStyles = result.Styles.Where(p => p.IsSelected).ToList();
        if (!result.OkPressed || selectedStyles.Count == 0)
        {
            return;
        }

        FileStyles.AddRange(selectedStyles);

        UpdateUsages();
    }

    private string MakeUniqueName(string name, ObservableCollection<StyleDisplay> styles)
    {
        var newName = name;
        if (styles.Any(p => p.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
        {
            var count = 2;
            var doRepeat = true;
            while (doRepeat)
            {
                newName = name + "_" + count;
                doRepeat = styles.Any(p => p.Name.Equals(newName, StringComparison.OrdinalIgnoreCase));
                count++;
            }
        }

        return newName;
    }

    [RelayCommand]
    private void FileNew()
    {
        var name = Se.Language.General.New;
        if (FileStyles.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            var count = 2;
            var doRepeat = true;
            while (doRepeat)
            {
                name = Se.Language.General.New + count;
                doRepeat = FileStyles.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                count++;
            }
        }

        var style = new SsaStyle { Name = name };
        FileStyles.Add(new StyleDisplay(style));
        UpdateUsages();
    }

    [RelayCommand]
    private void FileRemove()
    {
        DeleteFileStyle(SelectedFileStyle);
    }

    [RelayCommand]
    private void FileRemoveAll()
    {
        FileStyles.Clear();
    }

    [RelayCommand]
    private void FileCopy()
    {
        var selectedStyle = SelectedFileStyle;
        if (selectedStyle == null)
        {
            return;
        }

        var name = selectedStyle.Name + " - " + Se.Language.General.Copy;
        if (FileStyles.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            var count = 2;
            var doRepeat = true;
            while (doRepeat)
            {
                name = selectedStyle.Name + " - " + Se.Language.General.Copy + count;
                doRepeat = FileStyles.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                count++;
            }
        }

        var style = selectedStyle.ToSsaStyle();
        style.Name = name;
        FileStyles.Add(new StyleDisplay(style));

        UpdateUsages();
    }

    [RelayCommand]
    private async Task FileExport()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickSaveFile(Window, ".ass", "export-styles.ass", "Choose export file name");
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var styles = new List<SsaStyle>();
        foreach (var style in FileStyles)
        {
            styles.Add(style.ToSsaStyle());
        }

        var s = new Subtitle();
        s.Header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(
            AdvancedSubStationAlpha.DefaultHeader,
            styles);
        var text = s.ToText(new AdvancedSubStationAlpha());
        await System.IO.File.WriteAllTextAsync(fileName, text);
    }

    [RelayCommand]
    private async Task StorageImport()
    {
        if (Window == null)
        {
            return;
        }

        var format = new AdvancedSubStationAlpha();
        var fileName = await _fileHelper.PickOpenFile(Window, "Open subtitle file to import styles from", format.Name, "*" + format.Extension);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var s = Subtitle.Parse(fileName, format);
        var ssaStyles = AdvancedSubStationAlpha.GetSsaStylesFromHeader(s.Header);

        var result = await _windowService.ShowDialogAsync<AssaStylePickerWindow, AssaStylePickerViewModel>(Window, vm =>
        {
            vm.Initialize(ssaStyles.Select(p => new StyleDisplay(p) { IsSelected = true, Name = MakeUniqueName(p.Name, StorageStyles) }).ToList());
        });

        var selectedStyles = result.Styles.Where(p => p.IsSelected).ToList();
        if (!result.OkPressed || selectedStyles.Count == 0)
        {
            return;
        }

        StorageStyles.AddRange(selectedStyles);

        UpdateUsages();
    }

    [RelayCommand]
    private void StorageNew()
    {
        var name = Se.Language.General.New;
        if (StorageStyles.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            var count = 2;
            var doRepeat = true;
            while (doRepeat)
            {
                name = Se.Language.General.New + count;
                doRepeat = StorageStyles.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                count++;
            }
        }

        var style = new SsaStyle { Name = name };
        StorageStyles.Add(new StyleDisplay(style));
    }

    [RelayCommand]
    private void StorageRemove()
    {
        if (SelectedStorageStyle == null)
        {
            return;
        }

        var idx = StorageStyles.IndexOf(SelectedStorageStyle);
        StorageStyles.Remove(SelectedStorageStyle);
        SelectedStorageStyle = null;
        CurrentStyle = null;

        if (StorageStyles.Count > 0)
        {
            if (idx >= StorageStyles.Count)
            {
                idx = StorageStyles.Count - 1;
            }

            SelectedStorageStyle = StorageStyles[idx];
            CurrentStyle = SelectedStorageStyle;
        }

        StorageStyleGrid.Focus();
    }

    [RelayCommand]
    private void StorageRemoveAll()
    {
        StorageStyles.Clear();
    }

    [RelayCommand]
    private void StorageCopy()
    {
        var selectedStyle = SelectedStorageStyle;
        if (selectedStyle == null)
        {
            return;
        }

        var name = selectedStyle.Name + " - " + Se.Language.General.Copy;
        if (StorageStyles.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            var count = 2;
            var doRepeat = true;
            while (doRepeat)
            {
                name = selectedStyle.Name + " - " + Se.Language.General.Copy + count;
                doRepeat = StorageStyles.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                count++;
            }
        }

        var style = selectedStyle.ToSsaStyle();
        style.Name = name;
        StorageStyles.Add(new StyleDisplay(style));
    }

    [RelayCommand]
    private async Task StorageExport()
    {
        if (Window == null)
        {
            return;
        }

        var fileName = await _fileHelper.PickSaveFile(Window, ".ass", "export-styles.ass", "Choose export file name");
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var styles = new List<SsaStyle>();
        foreach (var style in StorageStyles)
        {
            styles.Add(style.ToSsaStyle());
        }

        var s = new Subtitle();
        s.Header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(
            AdvancedSubStationAlpha.DefaultHeader,
            styles);
        var text = s.ToText(new AdvancedSubStationAlpha());
        await System.IO.File.WriteAllTextAsync(fileName, text);
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() => { Window?.Close(); });
    }

    public void Initialize(Subtitle subtitle, SubtitleFormat format, string fileName, string selectedStyleName)
    {
        Title = string.Format(Se.Language.Assa.StylesTitleX, fileName);
        Header = subtitle.Header;
        _subtitle = subtitle;

        if (Header != null && Header.Contains("http://www.w3.org/ns/ttml"))
        {
            var s = new Subtitle { Header = Header };
            AdvancedSubStationAlpha.LoadStylesFromTimedText10(s, string.Empty, Header,
                AdvancedSubStationAlpha.HeaderNoStyles, new StringBuilder());
            Header = s.Header;
        }
        else if (Header != null && Header.StartsWith("WEBVTT", StringComparison.Ordinal))
        {
            _subtitle = WebVttToAssa.Convert(subtitle, new SsaStyle(), 0, 0);
            Header = _subtitle.Header;
        }

        if (Header == null || !Header.Contains("style:", StringComparison.OrdinalIgnoreCase))
        {
            ResetHeader();
        }

        FileStyles.Clear();
        foreach (var styleName in AdvancedSubStationAlpha.GetStylesFromHeader(Header))
        {
            var style = AdvancedSubStationAlpha.GetSsaStyle(styleName, Header);
            if (style != null)
            {
                var display = new StyleDisplay(style);
                SelectedBorderType = display.BorderStyle;
                FileStyles.Add(display);
            }
        }

        UpdateUsages();

        if (FileStyles.Count > 0)
        {
            SelectedFileStyle =
                FileStyles.FirstOrDefault(p => p.Name.Equals(selectedStyleName, StringComparison.OrdinalIgnoreCase));
            if (SelectedFileStyle == null)
            {
                SelectedFileStyle = FileStyles[0];
            }

            CurrentStyle = SelectedFileStyle;
            CurrentTitle = Se.Language.Assa.StylesInFile;
        }

        _timerUpdatePreview.Start();
    }

    private void UpdateUsages()
    {
        foreach (var style in FileStyles)
        {
            style.UsageCount =
                _subtitle.Paragraphs.Count(p => p.Extra.TrimStart('*').Equals(style.Name.TrimStart('*')));
        }
    }

    private void SaveFileStylesToHeader()
    {
        var styles = FileStyles.Select(p => p.ToSsaStyle()).ToList();
        Header = AdvancedSubStationAlpha.GetHeaderAndStylesFromAdvancedSubStationAlpha(Header, styles);
    }

    private void ResetHeader()
    {
        var format = new AdvancedSubStationAlpha();
        var sub = new Subtitle();
        var text = format.ToText(sub, string.Empty);
        var lines = text.SplitToLines();
        format.LoadSubtitle(sub, lines, string.Empty);
        Header = sub.Header;
    }

    private void SaveSettings()
    {
        Se.Settings.Assa.StoredStyles.Clear();
        foreach (var style in StorageStyles)
        {
            var s = new SeAssaStyle(style);
            Se.Settings.Assa.StoredStyles.Add(s);
        }
    }

    private void LoadSettings()
    {
        StorageStyles.Clear();
        foreach (var style in Se.Settings.Assa.StoredStyles)
        {
            var display = new StyleDisplay(style);
            StorageStyles.Add(display);
        }
    }

    private void UpdatePreview()
    {
        var style = CurrentStyle;
        if (style == null)
        {
            ImagePreview = new SKBitmap(1, 1, true).ToAvaloniaBitmap();
            return;
        }

        var text = "This is a test";

        var fontSize = (float)style.FontSize;
        SKBitmap bitmap;

        if (style.BorderStyle.Style == BorderStyleType.BoxPerLine)
        {
            bitmap = TextToImageGenerator.GenerateImageWithPadding(
                text,
                style.FontName,
                fontSize,
                style.Bold,
                style.ColorPrimary.ToSKColor(),
                style.ColorShadow.ToSKColor(),
                style.ColorOutline.ToSKColor(),
                style.ColorOutline.ToSKColor(),
                0,
                (float)style.ShadowWidth);

            if (style.ShadowWidth > 0)
            {
                bitmap = TextToImageGenerator.AddShadowToBitmap(bitmap,
                    (int)Math.Round(style.ShadowWidth, MidpointRounding.AwayFromZero), style.ColorShadow.ToSKColor());
            }
        }
        else if (style.BorderStyle.Style == BorderStyleType.OneBox)
        {
            bitmap = TextToImageGenerator.GenerateImageWithPadding(
                text,
                style.FontName,
                fontSize,
                style.Bold,
                style.ColorPrimary.ToSKColor(),
                style.ColorOutline.ToSKColor(),
                SKColors.Red,
                style.ColorShadow.ToSKColor(),
                (float)style.OutlineWidth,
                0,
                1.0f,
                (int)Math.Round(style.ShadowWidth));
        }
        else // FontBoxType.None
        {
            bitmap = TextToImageGenerator.GenerateImageWithPadding(
                text,
                style.FontName,
                fontSize,
                style.Bold,
                style.ColorPrimary.ToSKColor(),
                style.ColorOutline.ToSKColor(),
                style.ColorShadow.ToSKColor(),
                SKColors.Transparent,
                (float)style.OutlineWidth,
                (float)style.ShadowWidth);
        }

        ImagePreview = bitmap.ToAvaloniaBitmap();
    }

    internal void FileStylesChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selectedStyle = SelectedFileStyle;
        CurrentStyle = selectedStyle;
        CurrentTitle = Se.Language.Assa.StylesInFile;
        SelectedBorderType = selectedStyle?.BorderStyle ?? BorderTypes[0];
    }

    internal void StorageStylesChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selectedStyle = SelectedStorageStyle;
        CurrentStyle = selectedStyle;
        CurrentTitle = Se.Language.Assa.StylesSaved;
        SelectedBorderType = selectedStyle?.BorderStyle ?? BorderTypes[0];
    }

    internal void BorderTypeChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selectedStyle = CurrentStyle;
        if (selectedStyle == null)
        {
            return;
        }

        selectedStyle.BorderStyle = SelectedBorderType;
    }

    internal void FileStylesKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Delete)
        {
            var selectedStyle = SelectedFileStyle;
            DeleteFileStyle(selectedStyle);
            e.Handled = true;
        }
    }

    private void DeleteFileStyle(StyleDisplay? selectedStyle)
    {
        if (selectedStyle == null)
        {
            return;
        }

        Dispatcher.UIThread.Post(async void () =>
        {
            var answer = await MessageBox.Show(
                Window!,
                "Delete style?",
                $"Do you want to delete {selectedStyle.Name}?",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            if (answer != MessageBoxResult.Yes)
            {
                return;
            }

            if (selectedStyle != null)
            {
                var idx = FileStyles.IndexOf(selectedStyle);
                FileStyles.Remove(selectedStyle);
                SelectedFileStyle = null;
                CurrentStyle = null;
                if (FileStyles.Count > 0)
                {
                    if (idx >= FileStyles.Count)
                    {
                        idx = FileStyles.Count - 1;
                    }

                    SelectedFileStyle = FileStyles[idx];
                    CurrentStyle = SelectedFileStyle;
                }

                UpdateUsages();
            }

            FileStyleGrid.Focus();
        });
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }

    internal void FilesContextMenuOpening(object? sender, EventArgs e)
    {
        IsDeleteAllVisible = FileStyles.Count > 0;
        IsDeleteVisible = SelectedFileStyle != null;
    }
}