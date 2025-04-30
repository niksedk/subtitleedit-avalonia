using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts;

public partial class ShortcutsWindowViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> shortcuts;

    public ShortcutsWindowViewModel()
    {
        Shortcuts = new ObservableCollection<string> { "English", "Danish", "Spanish" };
    }
}