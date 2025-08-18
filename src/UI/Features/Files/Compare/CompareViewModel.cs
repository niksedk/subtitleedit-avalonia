using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Features.Main;
using Nikse.SubtitleEdit.Logic.Media;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Nikse.SubtitleEdit.Features.Files.Compare;

public partial class CompareViewModel : ObservableObject
{

    public ObservableCollection<SubtitleLineViewModel> LeftSubtitles { get; } = new();
    public ObservableCollection<SubtitleLineViewModel> RightSubtitles { get; } = new();

    [ObservableProperty]
    private SubtitleLineViewModel? selectedLeft;

    [ObservableProperty]
    private SubtitleLineViewModel? selectedRight;

    public Window? Window { get; internal set; }
    public bool OkPressed { get; private set; }



    private IFileHelper _fileHelper;
    private string _leftFileName = string.Empty;
    private string _rightFileName = string.Empty;

    public CompareViewModel(IFileHelper fileHelper)
    {
        _fileHelper = fileHelper;
    }

    internal void Initialize(ObservableCollection<SubtitleLineViewModel> left, ObservableCollection<SubtitleLineViewModel> right)
    {
        LeftSubtitles.Clear();
        foreach (var l in left)
        {
            LeftSubtitles.Add(l);
        }

        RightSubtitles.Clear();
        foreach (var r in right)
        {
            RightSubtitles.Add(r);
        }
    }

    [RelayCommand]
    private async Task PickLeftSubtitleFile()
    {
        var fileName = await _fileHelper.PickOpenSubtitleFile(Window!, "Open subtitle file");
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var subtitle = Subtitle.Parse(fileName);
        if (subtitle == null)
        {
            return;
        }

        LeftSubtitles.Clear();
        foreach (var line in subtitle.Paragraphs)
        {
            LeftSubtitles.Add(new SubtitleLineViewModel(line));
        }

        _leftFileName = fileName;
    }

    [RelayCommand]
    private async Task PickRightSubtitleFile()
    {
        var fileName = await _fileHelper.PickOpenSubtitleFile(Window!, "Open subtitle file");
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        var subtitle = Subtitle.Parse(fileName);
        if (subtitle == null)
        {
            return;
        }

        RightSubtitles.Clear();
        foreach (var line in subtitle.Paragraphs)
        {
            RightSubtitles.Add(new SubtitleLineViewModel(line));
        }

        _rightFileName = fileName;
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
}
