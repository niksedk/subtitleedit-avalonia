using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Declarative;
using Microsoft.Extensions.DependencyInjection;
using Nikse.SubtitleEdit.Features.Main.Layout;
using Nikse.SubtitleEdit.Logic.Config;

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
        _vm.MainView = this;
        DataContext = _vm;
        
        _vm.Window = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow!;

        _vm.Window.OnClosing(e =>
        {
            _vm.OnClosing();
        });

        _vm.Window.OnLoaded(e =>
        {
            _vm.OnLoaded();
        });

        var root = new DockPanel();

        // Menu bar
        root.Children.Add(InitMenu.Make(_vm).Dock(Dock.Top));

        // Toolbar
        root.Children.Add(InitToolbar.Make(_vm).Dock(Dock.Top));

        // Footer
        root.Children.Add(InitFooter.Make(_vm).Dock(Dock.Bottom));

        // Main content (fills all remaining space)
        _vm.ContentGrid = ViewContent.Make(_vm);
        InitLayout.MakeLayout(this, _vm, Se.Settings.General.LayoutNumber);
        root.Children.Add(_vm.ContentGrid);
        
        AddHandler(KeyUpEvent, _vm.OnKeyUpHandler, RoutingStrategies.Tunnel | RoutingStrategies.Bubble, handledEventsToo: true);

        return root;
    }
    
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _vm.KeyDown(e);
    }
}