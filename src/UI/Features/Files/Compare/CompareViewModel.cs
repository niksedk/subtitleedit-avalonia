using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nikse.SubtitleEdit.Features.Main;
using System.Collections.ObjectModel;

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

    public CompareViewModel()
    {
            
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
}
