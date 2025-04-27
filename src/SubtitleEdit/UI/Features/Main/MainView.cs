using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using Avalonia.Platform;
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
        
        // var icon = AssetLoader.Open(new Uri("avares://Nikse.SubtitleEdit/Assets/se.ico"));
        // var iconAsset = new WindowIcon(AssetLoader.Open(new Uri("avares://Nikse.SubtitleEdit/Assets/SE.png")));
        // this.Icon = iconAsset;
        // this.Icon = icon;

        _vm.Window = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow!;

        var root = new DockPanel();

        // Menu bar
        root.Children.Add(ViewMenu.Make(_vm).Dock(Dock.Top));

        // Toolbar
        root.Children.Add(ViewToolbar.Make(_vm).Dock(Dock.Top));

        // Footer
        root.Children.Add(ViewFooter.Make(_vm).Dock(Dock.Bottom));

        // Main content (fills all remaining space)
        _vm.ContentGrid = ViewContent.Make(_vm);
        InitLayout.MakeLayout(this, _vm, Se.Settings.General.LayoutNumber);
        root.Children.Add(_vm.ContentGrid);

        return root;
    }
}