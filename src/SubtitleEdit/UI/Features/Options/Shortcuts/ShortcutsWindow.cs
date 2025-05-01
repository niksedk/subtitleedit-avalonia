using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;

namespace Nikse.SubtitleEdit.Features.Options.Shortcuts;

public class ShortcutsWindow : Window
{
    private TextBox _searchBox;
    private ShortcutsViewModel _vm;
    
    public ShortcutsWindow(ShortcutsViewModel vm)
    {
        Title = "Shortcuts";
        Width = 500;
        Height = 600;
        CanResize = true;
        
        _searchBox = new TextBox
        {
            Watermark = "Search settings...",
            Margin = new Thickness(10),
        };
        
        var contentPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 15,
            Margin = new Thickness(10),
            Children = { new Label { Content = "Shortcuts:" } }
        };

        var scrollViewer = new ScrollViewer
        {
            Content = contentPanel,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };

        var buttonOk = new Button
        {
            Content = "OK",
        };

        var buttonCancel = new Button
        {
            Content = "Cancel",
        };
            
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 15,
            Margin = new Thickness(10),
            Children = { buttonOk, buttonCancel }
        };

        var dockPanel = new DockPanel
        {
            Children =
            {
                _searchBox,
                scrollViewer,
                buttonPanel,
            }
        };

        Content = dockPanel;

    }
}
