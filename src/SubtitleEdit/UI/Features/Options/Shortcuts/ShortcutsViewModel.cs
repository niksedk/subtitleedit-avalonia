using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts;

public partial class ShortcutsViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> shortcuts;

    public ShortcutsViewModel()
    {
        Shortcuts = new ObservableCollection<string> { "English", "Danish", "Spanish" };
    }
}