using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Features.Shared.PickMatroskaTrack;
using Nikse.SubtitleEdit.Logic;
using Nikse.SubtitleEdit.Logic.Config;
using Nikse.SubtitleEdit.Logic.Media;

namespace Nikse.SubtitleEdit.Features.Sync.PointSyncViaOther;

public partial class PointSyncViaOtherViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _subtitles;
    [ObservableProperty] private MatroskaTrackInfoDisplay? _selectedSubtitle;
    [ObservableProperty] private ObservableCollection<SubtitleLineViewModel> _othersubtitles;
    [ObservableProperty] private MatroskaTrackInfoDisplay? _selectedOtherSubtitle;
    [ObservableProperty] private string _fileName;
    [ObservableProperty] private string _fileNameOther;

    public Window? Window { get; set; }
    public bool OkPressed { get; private set; }
    public string WindowTitle { get; private set; }

    private readonly IFileHelper _fileHelper;
    private readonly IWindowService _windowService;

    private string _videoFileName;

    public PointSyncViaOtherViewModel(IFileHelper fileHelper, IWindowService windowService)
    {
        _fileHelper = fileHelper;
        _windowService = windowService;
        Subtitles =  new ObservableCollection<SubtitleLineViewModel>();
        Othersubtitles = new ObservableCollection<SubtitleLineViewModel>();
        WindowTitle = string.Empty;
        FileName = string.Empty;
        FileNameOther = string.Empty;
        _videoFileName = string.Empty;
    }
 
    public void Initialize(List<SubtitleLineViewModel> subtitles, string videoFileName, string fileName)
    {
        Subtitles.Clear();
        Subtitles.AddRange(subtitles);
        FileName = fileName;
        _videoFileName = videoFileName;
    }

    private void Close()
    {
        Dispatcher.UIThread.Post(() => { Window?.Close(); });
    }

    [RelayCommand]
    private async Task BrowseOther()
    {
        var fileName = await _fileHelper.PickOpenSubtitleFile(Window!, Se.Language.General.OpenSubtitleFileTitle);
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var subtitle = Subtitle.Parse(fileName);
        if (subtitle == null)
        {
            return;
        }

        FileNameOther = fileName;
        foreach (var p in subtitle.Paragraphs)   
        {
            Othersubtitles.Add(new SubtitleLineViewModel(p, subtitle.OriginalFormat));
        }
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

    internal void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Cancel();
        }
    }
}