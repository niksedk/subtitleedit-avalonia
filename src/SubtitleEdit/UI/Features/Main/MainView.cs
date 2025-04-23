using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using Avalonia.Media;

namespace Nikse.SubtitleEdit.Features.Main;

public class MainView : ViewBase
{
    protected override object Build()
    {
        var vm = new MainViewModel();
        DataContext = vm;
        
        var root = new DockPanel();

        // Menu bar
        root.Children.Add(ViewMenu.Make(vm).Dock(Dock.Top));

        // Toolbar
        root.Children.Add(ViewToolbar.Make(vm).Dock(Dock.Top));

        // Footer
        root.Children.Add(ViewFooter.Make(vm).Dock(Dock.Bottom));

        // Main content (fills all remaining space)
        root.Children.Add(ViewContent.Make(vm));

        return root;
    }
}