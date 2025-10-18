using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Nikse.SubtitleEdit.Logic;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts;

public partial class ShortcutTreeNode : ObservableObject
{
    [ObservableProperty] private string _category;
    [ObservableProperty] private string _title;
    [ObservableProperty] private StackPanel _textPanel;

    public ShortCut? ShortCut { get; set; }

    public ShortcutTreeNode(string category, string title, ShortCut shortcut)
    {
        Category = category;
        Title = title;
        ShortCut = shortcut;

        TextPanel = new StackPanel();

    }
}