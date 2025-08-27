using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

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
    [ObservableProperty] private ObservableCollection<string> _borderTypes;
    [ObservableProperty] private string _selectedBorderType;
    [ObservableProperty] private bool _isFileStylesFocused;

    public Window? Window { get; internal set; }
    public bool OkPressed { get; private set; }

    public IFileHelper _fileHelper;

    private string _fileName;
    private string _header;
    private Subtitle _subtitle;

    public AssaStylesViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;

        Title = string.Empty;
        FileStyles = new ObservableCollection<StyleDisplay>();
        StorageStyles = new ObservableCollection<StyleDisplay>();
        Fonts = new ObservableCollection<string>(FontHelper.GetSystemFonts());
        BorderTypes = new ObservableCollection<string>(new[] { Se.Language.General.Outline, Se.Language.General.Box, Se.Language.General.BoxPerLine });
        SelectedBorderType = BorderTypes[0];

        _fileName = string.Empty;
        _header = string.Empty;
        _subtitle = new Subtitle();

        LoadSettings();
    }

    [RelayCommand]
    private void Ok()
    {
        OkPressed = true;
        SaveSettings();
        Close();
    }

    [RelayCommand]
    private void Cancel()
    {
        Close();
    }

    [RelayCommand]
    private void FileImport()
    {
        UpdateUsages();
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
    }

    [RelayCommand]
    private void FileRemoveAll()
    {
    }

    [RelayCommand]
    private void FileCopy()
    {
    }

    [RelayCommand]
    private void FileExport()
    {
    }

    [RelayCommand]
    private void StorageImport()
    {
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
    }

    [RelayCommand]
    private void StorageRemoveAll()
    {
    }

    [RelayCommand]
    private void StorageCopy()
    {
    }

    [RelayCommand]
    private void StorageExport()
    {
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    public void Initialize(Subtitle subtitle, SubtitleFormat format, string fileName)
    {
        _header = subtitle.Header;

        if (_header != null && _header.Contains("http://www.w3.org/ns/ttml"))
        {
            var s = new Subtitle { Header = _header };
            AdvancedSubStationAlpha.LoadStylesFromTimedText10(s, string.Empty, _header, AdvancedSubStationAlpha.HeaderNoStyles, new StringBuilder());
            _header = s.Header;
        }
        else if (_header != null && _header.StartsWith("WEBVTT", StringComparison.Ordinal))
        {
            _subtitle = WebVttToAssa.Convert(subtitle, new SsaStyle(), 0, 0);
            _header = _subtitle.Header;
        }

        if (_header == null || !_header.Contains("style:", StringComparison.OrdinalIgnoreCase))
        {
            ResetHeader();
        }

        FileStyles.Clear();
        foreach (var styleName in AdvancedSubStationAlpha.GetStylesFromHeader(_header))
        {
            var style = AdvancedSubStationAlpha.GetSsaStyle(styleName, _header);
            if (style != null)
            {
                var display = new StyleDisplay(style);
                FileStyles.Add(display);
            }
        }      

        UpdateUsages();
    }

    private void UpdateUsages()
    {
        foreach (var style in FileStyles)
        {
            style.UsageCount = _subtitle.Paragraphs.Where(p => p.Style.Equals(style.Name)).Count();
        }
    }

    private void ResetHeader()
    {
        var format = new AdvancedSubStationAlpha();
        var sub = new Subtitle();
        var text = format.ToText(sub, string.Empty);
        var lines = text.SplitToLines();
        format.LoadSubtitle(sub, lines, string.Empty);
        _header = sub.Header;
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


    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }

    internal void FileStylesChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selectedStyle = SelectedFileStyle;
        CurrentStyle = selectedStyle;
    }
}
