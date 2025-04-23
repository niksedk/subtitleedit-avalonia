using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Avalonia.Themes.Fluent;
using Nikse.SubtitleEdit;
using Nikse.SubtitleEdit.Features.Main;

var lifetime = new ClassicDesktopStyleApplicationLifetime { Args = args, ShutdownMode = ShutdownMode.OnLastWindowClose };

var appBuilder = AppBuilder.Configure<Application>()
    .UsePlatformDetect()
    .AfterSetup(b => b.Instance?.Styles.Add(new FluentTheme()))
    .SetupWithLifetime(lifetime);

//var icon = AssetLoader.Open( new Uri($"avares://UI/avalonia-logo.ico"));

// Native menu setup
// var menu = new NativeMenu
// {
//     Items =
//     {
//         new NativeMenuItem("Open") { Click += OnOpenClick },
//         new NativeMenuItemSeparator(),
//         new NativeMenuItem("Close") { Click = (_, _) => Environment.Exit(0) }
//     }
// };

// appBuilder.Instance?.TrayIcon_Icons(
//     [
//         new TrayIcon()
//             .Icon(new WindowIcon(icon))
//             .Menu(menu)
//     ]
// );

// Main window setup
lifetime.MainWindow = new Window
{
    Title = "Avalonia markup samples",
    Content = new MainView(),
    // NativeMenu = menu
};

// lifetime.MainWindow = new Window()
//     .Title("Avalonia markup samples")
//     .Content(new MainView())
//     .NativeMenu_Menu(menu);

void OnOpenClick(EventArgs e)
{

}

#if DEBUG
lifetime.MainWindow.AttachDevTools();
#endif

lifetime.Start(args);