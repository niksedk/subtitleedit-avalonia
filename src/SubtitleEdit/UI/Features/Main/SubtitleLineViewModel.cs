using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Main;

public partial class SubtitleLineViewModel : ObservableObject
{
    [ObservableProperty]
    private int number;

    [ObservableProperty]
    private string startTime;

    [ObservableProperty]
    private string endTime;

    [ObservableProperty]
    private string duration;

    [ObservableProperty]
    private string text;

    [ObservableProperty]
    private bool isVisible;
}