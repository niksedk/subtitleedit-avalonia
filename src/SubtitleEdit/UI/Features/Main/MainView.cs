using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;

namespace Nikse.SubtitleEdit.Features.Main;


public static class Locator
{
    public static IServiceProvider Services { get; set; } = default!;
}
public class MainView : ViewBase
{
    private MainViewModel _vm;
    
    protected override object Build()
    {
        _vm = Locator.Services.GetRequiredService<MainViewModel>();
        
        DataContext = _vm;

        _vm.Window = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow!;

        var root = new DockPanel();

        // Menu bar
        root.Children.Add(ViewMenu.Make(_vm).Dock(Dock.Top));

        // Toolbar
        root.Children.Add(ViewToolbar.Make(_vm).Dock(Dock.Top));

        // Footer
        root.Children.Add(ViewFooter.Make(_vm).Dock(Dock.Bottom));

        // Main content (fills all remaining space)
        root.Children.Add(ViewContent.Make(_vm));

        return root;
    }
}