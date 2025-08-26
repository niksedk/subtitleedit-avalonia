using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using Nikse.SubtitleEdit.Features.Assa;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Media;
using System.Collections.ObjectModel;

namespace Nikse.SubtitleEdit.Features.Files.Statistics;

public partial class AssaStylesViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private ObservableCollection<StyleDisplay> _fileStyles;
    [ObservableProperty] private StyleDisplay? _selectedFileStyles;
    [ObservableProperty] private ObservableCollection<StyleDisplay> _storageStyles;
    [ObservableProperty] private StyleDisplay? _selectedStorageStyles;
    [ObservableProperty] private StyleDisplay? _currentStyle;
    [ObservableProperty] private ObservableCollection<string> _fonts;

    public Window? Window { get; internal set; }
    public bool OkPressed { get; private set; }

    public IFileHelper _fileHelper;

    private string _fileName;

    public AssaStylesViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;

        Title = string.Empty;
        FileStyles = new ObservableCollection<StyleDisplay>();
        StorageStyles = new ObservableCollection<StyleDisplay>();
        _fonts = new ObservableCollection<string>(FontHelper.GetSystemFonts());

        _fileName = string.Empty;
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
    private void FileImport()
    {
    }

    [RelayCommand]
    private void FileNew()
    {
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

    private void Close()
    {
        Dispatcher.UIThread.Post(() =>
        {
            Window?.Close();
        });
    }

    internal void Initialize(Subtitle subtitle, SubtitleFormat format, string fileName)
    {
    }

    internal void KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }
}
