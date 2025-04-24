using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Themes.Fluent;
using Microsoft.Extensions.DependencyInjection;
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


// Register all the services needed for the application to run
var collection = new ServiceCollection();
collection.AddCommonServices();
Locator.Services = collection.BuildServiceProvider();

var services = collection.BuildServiceProvider();
var vm = services.GetRequiredService<MainViewModel>();

// Main window setup
lifetime.MainWindow = new Window
{
    Title = "Subtitle Edit",
    //Content = new MainView(this),
    // NativeMenu = menu
};

lifetime.MainWindow.Content = new MainView();

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